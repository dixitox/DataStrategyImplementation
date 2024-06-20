/* eslint-disable @typescript-eslint/no-non-null-assertion, @typescript-eslint/no-explicit-any */
import React, { ChangeEvent, FormEvent, useCallback, useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { AssetItem } from "../../types/assetItem";
import { useMsal } from "@azure/msal-react";
import {
    Button,
    Input,
    InputOnChangeData,
    Label,
    Checkbox,
    CheckboxOnChangeData,
    RadioGroup,
    Radio,
    RadioGroupOnChangeData,
    Switch,
    SwitchOnChangeData,
} from "@fluentui/react-components";
import { AddRegular, DeleteRegular } from "@fluentui/react-icons";
import { useEffectOnce } from "../../utils/react/useEffectOnce";
import { httpClient } from "../../utils/httpClient/httpClient";
import { AssetTextInput } from "./assetTextInput";
import { AssetType } from "../../types/assetType";
import { formStyles } from "../../components/form/formStyles";
import { ChipData, InputChips } from "../../components/inputChips/inputChips";
import { AcrInfo } from "./acrInfo";
import { isPlatformAdmin } from "../../utils/auth/roles";
import { RunFlags } from "../../types/runFlags";
import { convertBase64Async } from "../../utils/react/misc";

class EditorFormDataStatic {
    static industryItems: string[] = [];
    static tagItems: string[] = [];
}

enum DemoSituation {
    DemoUrl = "demourl",
    HostDemo = "hostdemo",
    NoDemo = "nodemo",
}
interface ExtraData {
    demoSituation: DemoSituation;
}

interface EditorFormProps {
    asset: AssetItem | null;
    onSave: (asset: AssetItem) => void;
}

export function EditorForm({ asset, onSave: onSaveData }: EditorFormProps) {
    const { t } = useTranslation();
    const { accounts } = useMsal();
    const [data, setData] = useState<AssetItem>();
    const [extraData, setExtraData] = useState<ExtraData>();
    const [, setStaticDataLoaded] = useState(EditorFormDataStatic.industryItems.length > 0);
    const isAdmin = useRef<boolean>(false);
    const classes = formStyles();
    const rowClasses = "mt-4 flex";

    // Custom hook that can be used instead of useEffect() with zero dependencies.
    // Avoids a double execution of the effect when in React 18 DEV mode with <React.StrictMode>
    useEffectOnce(() => {
        if (EditorFormDataStatic.industryItems.length === 0 || EditorFormDataStatic.tagItems.length === 0)
            loadStaticDataAsync();
        isAdmin.current = isPlatformAdmin(accounts);
    });

    useEffect(() => {
        // Initialize form options from data received
        if (!asset) {
            setData({
                requireHosting: false,
                authors: [{}],
                assetType: [AssetType.Accelerator],
                industries: [],
                tags: [],
            } as unknown as AssetItem);
            setExtraData({ demoSituation: DemoSituation.DemoUrl });
        } else if (!data) {
            // Receive asset data
            setData(asset);
            setExtraData({
                demoSituation: asset.assetType.includes(AssetType.Demo)
                    ? asset.demoUrl
                        ? DemoSituation.DemoUrl
                        : DemoSituation.HostDemo
                    : DemoSituation.NoDemo,
            });
        }
    }, [asset]);

    async function loadStaticDataAsync() {
        // Wait for all promises in parallel
        await Promise.all([loadIndustriesAsync(), loadTagsAsync()]);
        setStaticDataLoaded(true);
    }

    async function loadIndustriesAsync() {
        const res: string[] = await httpClient.get(`${window.ENV.API_URL}/industries`);
        EditorFormDataStatic.industryItems = res.sort((a, b) => a.localeCompare(b));
    }
    async function loadTagsAsync() {
        const res: string[] = await httpClient.get(`${window.ENV.API_URL}/tags`);
        EditorFormDataStatic.tagItems = res.sort((a, b) => a.localeCompare(b));
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
                            ...(d![parts[0] as keyof AssetItem] as any),
                            [parts[1]]: input.value === undefined || input.value.length === 0 ? "" : input.value,
                        },
                    } as AssetItem)
            );
        } else
            setData(
                (d) =>
                    ({
                        ...d,
                        [property]: input.value === undefined || input.value.length === 0 ? "" : input.value,
                    } as AssetItem)
            );
    }, []);

    const onInputArrayChanged = useCallback((ev: ChangeEvent<HTMLElement>, input: InputOnChangeData): void => {
        // Example: input-authors[2].name or input-propname[1]
        const r = /(?:input-)?(\w+)\[(\d+)\]\.?(\w+)?/g;
        const match = r.exec((ev.target as HTMLInputElement).name);

        if (!match?.length) return;

        const [, property, index, field] = match;
        const idx = parseInt(index);

        setData((d) => {
            const propValues = d![property as keyof AssetItem] as any[];
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
            } as AssetItem;
        });
    }, []);

    const inputArrayAdd = useCallback((property: string): void => {
        setData(
            (d) =>
                ({
                    ...d,
                    [property]: [...(d![property as keyof AssetItem] as any[]), {}],
                } as unknown as AssetItem)
        );
    }, []);

    const inputArrayRemove = useCallback((property: string, index: number): void => {
        setData((d) => {
            const items = d![property as keyof AssetItem] as any[];
            items.splice(index, 1);
            return {
                ...d,
                [property]: [...items],
            } as unknown as AssetItem;
        });
    }, []);

    const onCheckboxChanged = useCallback((ev: ChangeEvent<HTMLInputElement>, input: CheckboxOnChangeData): void => {
        // For the checkboxes the property name is specified as name and the id is the value
        const property = ev.target.name;
        const item = ev.target.id;
        setData(
            (d) =>
                (input.checked
                    ? {
                          ...d,
                          [property]: [...((d?.[property as keyof AssetItem] as string[]) || []), item],
                      }
                    : {
                          ...d,
                          [property]: (d![property as keyof AssetItem] as string[]).filter((o) => o !== item),
                      }) as AssetItem
        );
    }, []);

    const onSwitchChanged = useCallback((ev: ChangeEvent<HTMLInputElement>, input: SwitchOnChangeData): void => {
        const property = ev.target.name;
        setData((d) => ({ ...d, [property]: input.checked } as AssetItem));
    }, []);

    const onDemoSituationChanged = useCallback((_ev: FormEvent<HTMLDivElement>, data: RadioGroupOnChangeData): void => {
        setExtraData((d) => ({ ...d, demoSituation: data.value as DemoSituation }));
        setData((d) => ({ ...d, requireHosting: data.value === DemoSituation.HostDemo } as AssetItem));
    }, []);

    const uploadFile = useCallback(async (ev: ChangeEvent<HTMLInputElement>): Promise<void> => {
        const file = ev.target.files![0];
        const base64 = await convertBase64Async(file);
        setData((d) => ({ ...d, screenshot: { base64: base64 } } as unknown as AssetItem));
    }, []);

    // #region Tags
    const onTagsChange = useCallback((chipsData: ChipData[]): void => {
        setData((d) => ({ ...d, tags: chipsData.map((c) => c.id) } as AssetItem));
    }, []);

    function tagsSuggestion(value: string): Promise<ChipData[]> {
        let search = value.trim().toLowerCase();
        return Promise.resolve(
            value.trim().length > 0
                ? EditorFormDataStatic.tagItems
                      .filter((tag) => tag.toLowerCase().includes(search))
                      .map((tag) => ({ id: tag, name: tag }))
                : EditorFormDataStatic.tagItems.map((tag) => ({ id: tag, name: tag }))
        );
    }
    // #endregion Tags

    function prepareAndSaveData(): void {
        if (!data) return;

        // Clean up condition dependent fields
        if (!data.assetType.includes(AssetType.Demo) || extraData?.demoSituation !== DemoSituation.DemoUrl)
            delete data["demoUrl"];
        if (!data.assetType.includes(AssetType.Demo) || extraData?.demoSituation !== DemoSituation.HostDemo)
            delete data["acrRepositoryName"];

        onSaveData(data);
    }

    return !data ? null : (
        <>
            <div className={rowClasses}>
                <Label className={classes.labelStyles}>{t("entities.asset.asset-type")}</Label>
                <div className="-ml-1.5 flex grow flex-wrap">
                    <Checkbox
                        className="w-60 self-center"
                        label={t("entities.asset.asset-type-accelerator")}
                        name="assetType"
                        id={AssetType.Accelerator}
                        onChange={onCheckboxChanged}
                        checked={data.assetType?.includes(AssetType.Accelerator)}
                    />
                    <Checkbox
                        className="w-60 self-center"
                        label={t("entities.asset.asset-type-demo")}
                        name="assetType"
                        id={AssetType.Demo}
                        onChange={onCheckboxChanged}
                        checked={data.assetType?.includes(AssetType.Demo)}
                    />
                </div>
            </div>
            {data.assetType?.includes(AssetType.Demo) && (
                <>
                    <div
                        className={rowClasses}
                        style={{ display: window.ENV?.RUN_FLAGS.includes(RunFlags.NoDemoHosting) ? "none" : "block" }}
                    >
                        <Label className={classes.labelStyles}></Label>
                        <div>
                            <Label className="font-semibold" htmlFor="input-demoSituation">
                                {t("pages.editor.demo-situation")}
                            </Label>
                            <RadioGroup
                                id="input-demoSituation"
                                name="demoSituation"
                                layout="horizontal"
                                className="mt-4 grow"
                                value={extraData?.demoSituation}
                                onChange={onDemoSituationChanged}
                            >
                                <Radio value={DemoSituation.DemoUrl} label={t("pages.editor.demo-situation-demourl")} />
                                <Radio
                                    value={DemoSituation.HostDemo}
                                    label={t("pages.editor.demo-situation-hostdemo")}
                                />
                                <AcrInfo className="-ml-3 -mt-1" />
                            </RadioGroup>
                        </div>
                    </div>

                    <div className={extraData?.demoSituation === DemoSituation.DemoUrl ? rowClasses : "hidden"}>
                        <AssetTextInput fieldName="demoUrl" value={data.demoUrl} onChange={onInputChanged} />
                    </div>
                    <div className={extraData?.demoSituation === DemoSituation.HostDemo ? rowClasses : "hidden"}>
                        <AssetTextInput
                            fieldName="acrRepositoryName"
                            value={data.acrRepositoryName}
                            onChange={onInputChanged}
                        />
                    </div>
                    <div className="border-b border-b-neutral-300 pb-4"></div>
                </>
            )}
            <div className={rowClasses}>
                <AssetTextInput fieldName="name" value={data.name} onChange={onInputChanged} />
            </div>

            {isAdmin.current && (
                <div className={rowClasses}>
                    <Label className={classes.labelStyles}>{t("entities.asset.enabled")}</Label>
                    <Switch
                        label={data.enabled ? t("common.yes") : t("common.no")}
                        checked={data.enabled}
                        name="enabled"
                        onChange={onSwitchChanged}
                    />
                </div>
            )}

            <div className={rowClasses}>
                <AssetTextInput fieldName="tagline" value={data.tagline} onChange={onInputChanged} />
            </div>

            <div className={rowClasses}>
                <Label className={classes.labelStyles}>{t("entities.asset.industries")}</Label>
                <div className="-ml-1.5 flex grow flex-wrap">
                    {EditorFormDataStatic.industryItems.map((item, idx) => {
                        return (
                            <Checkbox
                                className="w-60 self-center"
                                key={idx}
                                label={item}
                                name="industries"
                                id={item}
                                onChange={onCheckboxChanged}
                                checked={data.industries?.includes(item)}
                            />
                        );
                    })}
                </div>
            </div>

            {/* <div className={rowClasses}>
                <Label className={classes.labelStyles}>{t("entities.asset.tags")}</Label>
                <div className="-ml-1.5 flex grow flex-wrap">
                    {EditorFormDataStatic.tagItems.map((item, idx) => {
                        return (
                            <Checkbox
                                className="w-60 self-center"
                                key={idx}
                                label={item}
                                name="tags"
                                id={item}
                                onChange={onCheckboxChanged}
                                checked={data.tags?.includes(item)}
                            />
                        );
                    })}
                </div>
            </div> */}

            <div className={rowClasses}>
                <Label className={classes.labelStyles}>{t("entities.asset.tags")}</Label>
                <div className="grow">
                    <InputChips
                        fetchSearchSuggestions={tagsSuggestion}
                        onChange={onTagsChange}
                        chipsData={data.tags?.map((tag) => ({ id: tag, name: tag }))}
                    />
                </div>
            </div>

            <div className={rowClasses}>
                <Label className={classes.labelStyles} style={{ paddingTop: "6px" }}>
                    {t("entities.asset.author_other")}
                </Label>
                <div className="grow">
                    {data.authors.map((item, idx) => {
                        return (
                            <div className="mb-4 flex" key={idx}>
                                <Input
                                    className="mr-4 w-[60%]"
                                    name={`authors[${idx}].name`}
                                    onChange={onInputArrayChanged}
                                    value={item.name || ""}
                                    type="text"
                                    maxLength={100}
                                    placeholder={t("entities.asset.author-name")}
                                />
                                <Input
                                    className="mr-4 w-[40%]"
                                    name={`authors[${idx}].gitHubAlias`}
                                    onChange={onInputArrayChanged}
                                    value={item.gitHubAlias || ""}
                                    type="text"
                                    maxLength={50}
                                    placeholder={t("entities.asset.author-git-hub-alias")}
                                />
                                
                                <Input
                                    className="mr-4 w-[40%]"
                                    name={`authors[${idx}].linkedin`}
                                    onChange={onInputArrayChanged}
                                    value={item.linkedin || ""}
                                    type="text"
                                    maxLength={50}
                                    placeholder={t("entities.asset.author-linkedin")}
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
                <AssetTextInput
                    fieldName="repositoryUrl"
                    value={data.repositoryUrl}
                    placeholder={t("entities.asset.repository-url-placeholder")}
                    onChange={onInputChanged}
                    extendedInfo={<div dangerouslySetInnerHTML={{ __html: t("entities.asset.repository-url-info") }} />}
                />
            </div>

            {window.ENV?.RUN_FLAGS.includes(RunFlags.WorkflowsEnabled) && (
                <>
                    <div className={rowClasses}>
                        <AssetTextInput
                            fieldName="workflowUrl"
                            value={data.workflowUrl}
                            placeholder={t("entities.asset.workflow-url-placeholder")}
                            onChange={onInputChanged}
                        />
                    </div>
                </>
            )}

            <div className={rowClasses}>
                <AssetTextInput
                    fieldName="armTemplate"
                    value={data.armTemplate}
                    placeholder={t("entities.asset.arm-template-placeholder")}
                    onChange={onInputChanged}
                />
            </div>

            <div className={rowClasses}>
                <AssetTextInput
                    fieldName="businessProblem"
                    value={data.businessProblem}
                    multiline={true}
                    onChange={onInputChanged}
                    placeholder={t("entities.asset.business-problem-placeholder")}
                />
            </div>

            <div className={rowClasses}>
                <AssetTextInput
                    fieldName="businessValue"
                    value={data.businessValue}
                    multiline={true}
                    onChange={onInputChanged}
                    placeholder={t("entities.asset.business-value-placeholder")}
                />
            </div>

            <div className={rowClasses}>
                <AssetTextInput
                    fieldName="description"
                    value={data.description}
                    multiline={true}
                    onChange={onInputChanged}
                    placeholder={t("entities.asset.description-placeholder")}
                />
            </div>

            <div className={rowClasses}>
                <AssetTextInput
                    fieldName="modelingApproachAndTraining"
                    value={data.modelingApproachAndTraining}
                    multiline={true}
                    onChange={onInputChanged}
                    placeholder={t("entities.asset.modeling-approach-and-training-placeholder")}
                />
            </div>

            <div className={rowClasses}>
                <AssetTextInput
                    fieldName="data"
                    value={data.data}
                    multiline={true}
                    onChange={onInputChanged}
                    placeholder={t("entities.asset.data-placeholder")}
                />
            </div>

            <div className={rowClasses}>
                <AssetTextInput
                    fieldName="architecture"
                    value={data.architecture}
                    multiline={true}
                    onChange={onInputChanged}
                    placeholder={t("entities.asset.architecture-placeholder")}
                />
            </div>

            <div className={rowClasses}>
                <Label className={classes.labelStyles}>{t("entities.asset.screenshot")}</Label>

                <div className="flex-start flex grow flex-col items-start">
                    <div className="flex w-full grow items-baseline">
                        <input
                            id="screenshot"
                            type="file"
                            accept="image/png, image/gif, image/jpeg"
                            className="hidden"
                            onChange={uploadFile}
                        />
                        <Button
                            className="h-8"
                            appearance="secondary"
                            onClick={() => document.getElementById("screenshot")!.click()}
                        >
                            {t("common.upload")}
                        </Button>
                        {(data.screenshot?.base64 || data.screenshot?.path) && (
                            <Input
                                className="mt-1 ml-2 grow"
                                name="screenshot.alternativeText"
                                onChange={onInputChanged}
                                value={data.screenshot?.alternativeText || ""}
                                type="text"
                                maxLength={100}
                                placeholder={t("entities.asset.alternative-text")}
                            />
                        )}
                    </div>
                    <img
                        className="mt-4 max-h-[200px] object-contain"
                        src={
                            data.screenshot?.base64 ||
                            (data.screenshot?.path ? `${window.ENV.IMG_URL}/${data.screenshot.path}` : "")
                        }
                        alt={data.screenshot?.alternativeText || ""}
                    />
                </div>
            </div>

            <div className={rowClasses}>
                <AssetTextInput
                    fieldName="comments"
                    value={data.comments}
                    multiline={true}
                    onChange={onInputChanged}
                    placeholder={t("entities.asset.comments-placeholder")}
                />
            </div>

            <div className={rowClasses}>
                <Button appearance="primary" onClick={prepareAndSaveData}>
                    {t("common.submit")}
                </Button>
            </div>
        </>
    );
}
