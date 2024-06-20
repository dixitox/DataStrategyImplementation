import React, { useState } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router-dom";
import { AssetItem } from "../../types/assetItem";
import { makeStyles, Radio, RadioGroup, Spinner } from "@fluentui/react-components";
import { useEffectOnce } from "../../utils/react/useEffectOnce";
import { httpClient, parseHttpException } from "../../utils/httpClient/httpClient";
import { HeaderBar, NavLocation } from "../../components/headerBar/headerBar";
import { SearchBox } from "../../components/searchBox/searchBox";
import { EditorForm } from "./editorForm";
import { ContactForm } from "./contactForm";
import { ContactItem } from "../../types/contactItem";
import { Header } from "../../components/header/header";
import { enqueueSnackbar } from "notistack";

enum EditorMode {
    New = "create",
    Update = "update",
    Contact = "contact",
}

const useStyles = makeStyles({
    // Fix vertical alignment of radio buttons labels on tabs
    radioLabels: { paddingTop: "6px", paddingBottom: "6px", height: "100%" },
});

export function AssetEditor() {
    const [data, setData] = useState<AssetItem | null>();
    const [isLoading, setIsLoading] = useState<boolean>(false);

    const navigate = useNavigate();
    const { t } = useTranslation();
    const params = useParams();
    const classes = useStyles();
    const showTabs = !params.assetId;
    const title =
        params.assetId === "new"
            ? t("pages.editor.title-new")
            : params.assetId
            ? t("pages.editor.title-update")
            : t("pages.editor.how-to");

    const [mode, setMode] = useState<EditorMode>(
        params.assetId === "new" ? EditorMode.New : params.assetId ? EditorMode.Update : EditorMode.Contact
    );

    // Custom hook that can be used instead of useEffect() with zero dependencies.
    // Avoids a double execution of the effect when in React 18 DEV mode with <React.StrictMode>
    useEffectOnce(() => {
        if (params.assetId && params.assetId !== "new") loadDataAsync(params.assetId);
        else setData(null);
    });

    function onSearchChanged(searchValue: string): void {
        if (searchValue) navigate(`/search?q=${encodeURIComponent(searchValue)}`);
    }

    async function loadDataAsync(assetId: string) {
        setIsLoading(true);
        const res: AssetItem = await httpClient.get(`${window.ENV.API_URL}/assets/${assetId}`);
        setData(res);
        setIsLoading(false);
    }

    async function saveDataAsync(asset: AssetItem) {
        setIsLoading(true);
        setData(asset);

        try {
            if (!asset?.id) {
                await httpClient.post(`${window.ENV.API_URL}/assets`, asset);
                enqueueSnackbar(t("pages.editor.submitted-ok"), { variant: "success" });
                navigate("/");
            } else {
                await httpClient.put(`${window.ENV.API_URL}/assets/${asset.id}`, asset);
                enqueueSnackbar(t("pages.editor.updated-ok"), { variant: "success" });
                navigate(`/assets/${asset.id}`);
            }
        } catch (ex) {
            const messages = parseHttpException(ex, t);
            enqueueSnackbar("", { variant: "error", persist: false, msgItems: messages });
        }

        setIsLoading(false);
    }

    async function saveContactAsync(contact: ContactItem) {
        setIsLoading(true);
        try {
            await httpClient.post(`${window.ENV.API_URL}/contacts`, contact);
            enqueueSnackbar(t("pages.editor.contact-sent"), { variant: "success" });
            navigate("/");
        } catch (ex) {
            const messages = parseHttpException(ex, t);
            enqueueSnackbar("", { variant: "error", persist: false, msgItems: messages });
        }
        setIsLoading(false);
    }

    return (
        <>
            <Header>
                <HeaderBar location={NavLocation.Contribute} />
                <SearchBox
                    className="items-center justify-end"
                    labelClassName="font-semibold"
                    inputClassName="w-40"
                    onSearchChanged={onSearchChanged}
                    placeholder={t("common.search")}
                />
            </Header>

            <main className="grid grid-cols-1 gap-4 px-8 md:grid-cols-4 md:gap-x-12 md:px-24">
                {/* Left nav bar */}
                <div className="whitespace-nowrap md:sticky md:top-5 md:self-start">
                    <div className="w-fit border-t-4 py-6 text-sm font-bold uppercase">
                        {t("pages.editor.page-content")}
                    </div>
                    <div className="mb-4 text-base font-semibold leading-8">
                        <a href="#top">
                            <div>{title}</div>
                        </a>
                    </div>
                </div>

                <div className="col-span-3">
                    <h1 id="top" className="-mt-3 mb-6">
                        {title}
                    </h1>

                    {isLoading && (
                        <div className="mt-16">
                            <Spinner size="extra-large" />
                        </div>
                    )}
                    {/* Form body */}
                    {data !== undefined && (
                        <div className={isLoading ? "hidden" : "block"}>
                            {showTabs && (
                                <RadioGroup layout="horizontal">
                                    {/* Outer box with an "overflow: hidden" to hide bottom shadow */}
                                    <div className="-m-4 mb-0 flex grow gap-2.5 overflow-hidden p-4 pb-0">
                                        <div
                                            className={`flex grow basis-0 justify-center rounded-t-xl p-4 shadow-lg ${
                                                mode === EditorMode.Contact ? "bg-white" : "bg-neutral-200"
                                            }`}
                                        >
                                            <Radio
                                                value={EditorMode.Contact}
                                                label={{
                                                    className: classes.radioLabels,
                                                    children: t("pages.editor.contribute-other"),
                                                }}
                                                checked={mode === EditorMode.Contact}
                                                onChange={(_, data) => setMode(data.value as EditorMode)}
                                            />
                                        </div>
                                        <div
                                            className={`flex grow basis-0 justify-center rounded-t-xl p-4 shadow-lg ${
                                                mode !== EditorMode.Contact ? "bg-white" : "bg-neutral-200"
                                            }`}
                                        >
                                            <Radio
                                                value={EditorMode.New}
                                                label={{
                                                    className: classes.radioLabels,
                                                    children: t("pages.editor.contribute-new"),
                                                }}
                                                checked={mode === EditorMode.New}
                                                onChange={(_, data) => setMode(data.value as EditorMode)}
                                            />
                                        </div>
                                    </div>
                                </RadioGroup>
                            )}

                            <div className="rounded-b-xl bg-white p-7 shadow-lg">
                                {mode !== EditorMode.Contact ? (
                                    <EditorForm asset={data} onSave={saveDataAsync} />
                                ) : (
                                    <ContactForm onSave={saveContactAsync} />
                                )}
                            </div>
                        </div>
                    )}
                </div>
            </main>
        </>
    );
}
