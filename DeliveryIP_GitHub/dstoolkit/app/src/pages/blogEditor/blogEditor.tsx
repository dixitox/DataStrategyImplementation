import React, { ChangeEvent, Suspense, useCallback, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { Link, useNavigate, useParams } from "react-router-dom";
import { useEffectOnce } from "../../utils/react/useEffectOnce";
import { HeaderBar } from "../../components/headerBar/headerBar";
import { Header } from "../../components/header/header";
import { LexEditorHandle } from "../../components/lexEditor/lexEditor";
import { BlogEntry } from "../../types/blogEntry";
import { httpClient, parseHttpException } from "../../utils/httpClient/httpClient";
import { Spinner } from "@fluentui/react-spinner";
import { convertBase64Async, sleep } from "../../utils/react/misc";
import { Button, Dropdown, Option, Input, InputOnChangeData, Label, Textarea } from "@fluentui/react-components";
import { formStyles } from "../../components/form/formStyles";
import sanitizeHtml from "sanitize-html";
import { BlogEntryType } from "../../types/blogEntryType";
import { BlogEntryStatus } from "../../types/blogEntryStatus";
import { enqueueSnackbar } from "notistack";
import { InfoPopover } from "../../components/infoPopover/infoPopover";
import { AddRegular, DeleteRegular } from "@fluentui/react-icons";

const LexEditor = React.lazy(() => import("../../components/lexEditor/lexEditor"));

export function BlogEditor() {
    const navigate = useNavigate();
    const { t } = useTranslation();
    const params = useParams();
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const [data, setData] = useState<BlogEntry>({} as BlogEntry);
    const lexEditorRef = useRef<LexEditorHandle>(null);
    const classes = formStyles();
    const rowClasses = "mt-4 flex";

    // Custom hook that can be used instead of useEffect() with zero dependencies.
    // Avoids a double execution of the effect when in React 18 DEV mode with <React.StrictMode>
    useEffectOnce(() => {
        if (params.blogId) 
            loadDataAsync(params.blogId);
        else 
            setData({authors: [{}]} as unknown as BlogEntry);
    });

    async function loadDataAsync(blogId: string) {
        setIsLoading(true);
        const res: BlogEntry = await httpClient.get(`${window.ENV.API_URL}/blogentries/${blogId}`);

        setData(res);

        if (res.body && !res.body?.startsWith("<p")) res.body = `<p class=\"richtext-paragraph\">${res.body}</p>`;
        if (res?.body) {
            let retry = 1;
            // The editor is lazy loaded and might not be available yet
            while (!lexEditorRef.current && retry < 10) {
                await sleep(retry * 250);
                retry++;
            }
            if (lexEditorRef.current) lexEditorRef.current?.setHtml(res.body);
        }
        setIsLoading(false);
    }

    async function saveDataAsync() {
        setIsLoading(true);
        try {
            data.body = sanitizeHtml((await lexEditorRef.current?.getHtmlAsync()) || "", {
                allowedTags: sanitizeHtml.defaults.allowedTags.concat(["img"]),
                allowedClasses: { "*": ["richtext-*"] },
            });
            if (!data?.id) {
                await httpClient.post(`${window.ENV.API_URL}/blogentries`, data);
                enqueueSnackbar(t("pages.blog-editor.submitted-ok"), { variant: "success" });
                navigate("/blog");
            } else {
                await httpClient.put(`${window.ENV.API_URL}/blogentries/${data.id}`, data);
                enqueueSnackbar(t("pages.blog-editor.updated-ok"), { variant: "success" });
                navigate(`/blog/${data.id}`);
            }
        } catch (ex) {
            const messages = parseHttpException(ex, t);
            enqueueSnackbar("", { variant: "error", persist: false, msgItems: messages });
        }

        setIsLoading(false);
    }

    const onInputChanged = useCallback((ev: ChangeEvent<HTMLElement>, input: InputOnChangeData): void => {
        const property = (ev.target as HTMLInputElement).name;
        if (property.includes(".")) {
            // e.g. screenshot.alttext
            const parts = property.split(".");
            setData(
                (d) =>
                    ({
                        ...d,
                        [parts[0]]: {
                            ...(d![parts[0] as keyof BlogEntry] as any),
                            [parts[1]]: input.value === undefined || input.value.length === 0 ? "" : input.value,
                        },
                    } as BlogEntry)
            );
        } else
            setData(
                (d) =>
                    ({
                        ...d,
                        [property]: input.value === undefined || input.value.length === 0 ? "" : input.value,
                    } as BlogEntry)
            );
    }, []);

    function onTypeSelect(_event: unknown, data: { optionValue?: string }) {
        setData((d) => ({ ...d, entryType: data.optionValue } as BlogEntry));
    }

    function onStatusSelect(_event: unknown, data: { optionValue?: string }) {
        setData((d) => ({ ...d, status: data.optionValue } as BlogEntry));
    }

    const uploadFile = useCallback(async (ev: ChangeEvent<HTMLInputElement>): Promise<void> => {
        const file = ev.target.files![0];
        const base64 = await convertBase64Async(file);
        setData((d) => ({ ...d, heroImage: { ...d?.heroImage, base64: base64 } } as unknown as BlogEntry));
    }, []);

    const uploadContentImage = useCallback(async (base64Image: string | ArrayBuffer | null): Promise<string> => {
        const res: string = await httpClient.post(`${window.ENV.API_URL}/blogentries/images`, { base64: base64Image });
        return `${window.ENV.IMG_URL}/${res}`;
    }, []);

    const onInputArrayChanged = useCallback((ev: ChangeEvent<HTMLElement>, input: InputOnChangeData): void => {
        // Example: input-authors[2].name or input-propname[1]
        const r = /(?:input-)?(\w+)\[(\d+)\]\.?(\w+)?/g;
        const match = r.exec((ev.target as HTMLInputElement).name);

        if (!match?.length) return;

        const [, property, index, field] = match;
        const idx = parseInt(index);

        setData((d) => {
            const propValues = d![property as keyof BlogEntry] as any[];
            if (input.value) {
                while (idx >= propValues.length) {
                    if (field) propValues.push({});
                    else propValues.push("");
                }
            }
            if (field) propValues[idx][field as keyof any[]] = input.value;
            else propValues[idx] = input.value;
            return {
                ...d,
                [property]: [...propValues],
            } as BlogEntry;
        });
    }, []);

    const inputArrayAdd = useCallback((property: string): void => {
        setData(
            (d) =>
                ({
                    ...d,
                    [property]: [...(d![property as keyof BlogEntry] as any[]), {}],
                } as unknown as BlogEntry)
        );
    }, []);

    const inputArrayRemove = useCallback((property: string, index: number): void => {
        setData((d) => {
            const items = d![property as keyof BlogEntry] as any[];
            items.splice(index, 1);
            return {
                ...d,
                [property]: [...items],
            } as unknown as BlogEntry;
        });
    }, []);

    return (
        <>
            <Header>
                <HeaderBar />
            </Header>

            <main className="grid grid-cols-1 gap-4 px-8 md:grid-cols-4 md:gap-x-12 md:px-24">
                {/* Left nav bar */}
                <div className="whitespace-nowrap md:sticky md:top-5 md:self-start">
                    <div className="w-fit border-t-4 py-6 text-sm font-bold uppercase">
                        {t("pages.blog-editor.title-left")}
                    </div>
                    {/* <div className="text-base font-semibold leading-8">
                        <a href="#topic">
                            <div>topic description</div>
                        </a>
                    </div> */}
                </div>

                <div className="col-span-3">
                    <h1 className="-mt-3 mb-6">
                        {params.blogId ? t("pages.blog-editor.title-update") : t("pages.blog-editor.title-create")}
                    </h1>
                    {isLoading && (
                        <div className="my-4">
                            <Spinner size="extra-large" />
                        </div>
                    )}

                    {/* Form body */}
                    <div className="overflow-hidden rounded-b-xl bg-white p-7 shadow-lg">
                        <div className={rowClasses}>
                            <Label className={classes.labelStyles} htmlFor="input-title">
                                {t("entities.blog.title")}
                            </Label>
                            <Input
                                name="title"
                                id="input-title"
                                className="grow"
                                onChange={onInputChanged}
                                value={data?.title || ""}
                                type="text"
                                maxLength={100}
                                placeholder={t("entities.blog.title")}
                            />
                        </div>
                        <div className={rowClasses}>
                            <Label className={classes.labelStyles}>{t("entities.blog.entrytype")}</Label>
                            <Dropdown
                                onOptionSelect={onTypeSelect}
                                selectedOptions={[data?.entryType || BlogEntryType.Announcement]}
                                value={t(
                                    `entities.blog.entrytype-${(
                                        data?.entryType || BlogEntryType.Announcement
                                    ).toLocaleLowerCase()}`
                                )}
                            >
                                {Object.values(BlogEntryType).map((o) => (
                                    <Option
                                        key={o}
                                        value={o}
                                        text={t(`entities.blog.entrytype-${o.toLocaleLowerCase()}`)!} // eslint-disable-line @typescript-eslint/no-non-null-assertion
                                    >
                                        {t(`entities.blog.entrytype-${o.toLocaleLowerCase()}`)}
                                    </Option>
                                ))}
                            </Dropdown>
                        </div>
                        <div className={rowClasses}>
                            <Label className={classes.labelStyles}>{t("entities.blog.status")}</Label>
                            <Dropdown
                                onOptionSelect={onStatusSelect}
                                selectedOptions={[data?.status || BlogEntryStatus.Draft]}
                                value={t(
                                    `entities.blog.status-${(
                                        data?.status || BlogEntryStatus.Draft
                                    ).toLocaleLowerCase()}`
                                )}
                            >
                                {Object.values(BlogEntryStatus).map((o) => (
                                    <Option
                                        key={o}
                                        value={o}
                                        text={t(`entities.blog.status-${o.toLocaleLowerCase()}`)!} // eslint-disable-line @typescript-eslint/no-non-null-assertion
                                    >
                                        {t(`entities.blog.status-${o.toLocaleLowerCase()}`)}
                                    </Option>
                                ))}
                            </Dropdown>
                        </div>
                        <div className={rowClasses}>
                            <Label className={classes.labelStyles} htmlFor="input-introduction">
                                {t("entities.blog.introduction")}
                                <InfoPopover className="-mt-3 -ml-1">{t("entities.blog.introduction-infopopover")}</InfoPopover>
                            </Label>
                            <Textarea
                                name="introduction"
                                id="input-introduction"
                                className="grow"
                                onChange={onInputChanged}
                                value={data?.introduction || ""}
                                maxLength={400}
                                placeholder={t("entities.blog.introduction")}
                                textarea={{ className: classes.textAreaStyles }}
                            />
                        </div>

                        <div className={rowClasses}>
                            <Label className={classes.labelStyles}>{t("entities.blog.hero-image")}</Label>

                            <div className="flex-start flex grow flex-col items-start">
                                <div className="flex w-full grow items-baseline">
                                    <input
                                        id="input-image"
                                        type="file"
                                        accept="image/png, image/gif, image/jpeg"
                                        className="hidden"
                                        onChange={uploadFile}
                                    />
                                    <Button
                                        className="h-8"
                                        appearance="secondary"
                                        onClick={() => document.getElementById("input-image")!.click()}
                                    >
                                        {t("common.upload")}
                                    </Button>
                                    {(data.heroImage?.base64 || data.heroImage?.path) && (
                                        <Input
                                            className="mt-1 ml-2 grow"
                                            name="heroImage.alternativeText"
                                            onChange={onInputChanged}
                                            value={data.heroImage?.alternativeText || ""}
                                            type="text"
                                            maxLength={100}
                                            placeholder={t("entities.blog.alternative-text")}
                                        />
                                    )}
                                </div>
                                <img
                                    className="m-auto mt-4 h-auto w-full rounded-xl object-cover"
                                    src={
                                        data.heroImage?.base64 ||
                                        (data.heroImage?.path ? `${window.ENV.IMG_URL}/${data.heroImage.path}` : "")
                                    }
                                    alt={data.heroImage?.alternativeText || ""}
                                />
                            </div>
                        </div>

               <div className={rowClasses}>
                <Label className={classes.labelStyles} style={{ paddingTop: "6px" }}>
                    {t("entities.blog.author_other")}
                </Label>
                <div className="grow">
                    {data.authors?.map((item, idx) => {
                        return (
                            <div className="mb-4 flex" key={idx}>
                                <Input
                                    className="mr-4 w-[60%]"
                                    name={`authors[${idx}].name`}
                                    onChange={onInputArrayChanged}
                                    value={item.name || ""}
                                    type="text"
                                    maxLength={100}
                                    placeholder={t("entities.blog.author-name")}
                                />
                                
                                <Input
                                    className="mr-4 w-[40%]"
                                    name={`authors[${idx}].linkedin`}
                                    onChange={onInputArrayChanged}
                                    value={item.linkedin || ""}
                                    type="text"
                                    maxLength={50}
                                    placeholder={t("entities.blog.author-linkedin")}
                                />

                                <Button
                                    disabled={idx === 0}
                                    icon={<DeleteRegular />}
                                    onClick={() => inputArrayRemove("authors", idx)}
                                />
                            </div>
                        );
                    })}
                    <Button icon={<AddRegular />} onClick={() => inputArrayAdd("authors")} />
                </div>
            </div>



                        <div className={rowClasses}>
                            {/* <Label className={classes.labelStyles}>{t("entities.blog.body")}</Label> */}
                        </div>
                        <Suspense fallback={<div className="my-4">{t("common.loading")}</div>}>
                            <LexEditor
                                ref={lexEditorRef}
                                placeholder={t("pages.blog-editor.placeholder")}
                                uploadImage={uploadContentImage}
                            />
                        </Suspense>
                        <div className={rowClasses}>
                            <Button appearance="primary" onClick={saveDataAsync} disabled={isLoading}>
                                {params.blogId ? t("common.save") : t("common.submit")}
                            </Button>
                        </div>
                    </div>
                </div>
            </main>
        </>
    );
}
