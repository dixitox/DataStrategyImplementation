import React, { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { HeaderBar, NavLocation } from "../../components/headerBar/headerBar";
import { Paged } from "../../types/paged";
import {
    Button,
    DialogOpenChangeData,
    DialogOpenChangeEvent,
    Spinner,
    ToggleButton,
    Tooltip,
} from "@fluentui/react-components";
import { Link } from "react-router-dom";
import { AssetManageItem } from "../../types/assetManageItem";
import { httpClient } from "../../utils/httpClient/httpClient";
import { TableSimpleFilled, TextBulletListLtrFilled } from "@fluentui/react-icons";
import { StateFilter } from "./stateFilter";
import { ViewAsCards } from "./viewAsCards";
import { ViewAsList } from "./viewAsList";
import { DialogConfirm } from "../../components/dialogConfirm/dialogConfirm";
import { Header } from "../../components/header/header";

enum ViewMode {
    List = 0,
    Cards = 1,
}

export function Manage() {
    const { t } = useTranslation();
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const [filterState, setFilterState] = useState<boolean | undefined>(undefined);
    const [queryResults, setQueryResults] = useState<Paged<AssetManageItem, null>>({} as Paged<AssetManageItem, null>);
    const [viewMode, setViewMode] = useState<ViewMode>(ViewMode.List);
    const [dialogDeleteId, setDialogDeleteId] = useState<string | undefined>(undefined);

    // Custom hook that can be used instead of useEffect() with zero dependencies.
    // Avoids a double execution of the effect when in React 18 DEV mode with <React.StrictMode>
    // useEffectOnce(() => {
    //     loadDataAsync();
    // });

    async function loadDataAsync() {
        setIsLoading(true);

        const result: Paged<AssetManageItem, null> = await httpClient.get(
            `${window.ENV.API_URL}/assets/administrables?pageSize=100&page=1&enabled=${filterState !== undefined ? filterState : ""}`
        );

        setQueryResults(result);
        setIsLoading(false);
    }

    useEffect(() => {
        loadDataAsync();
    }, [filterState]);

    function onFilterChanged(filter?: boolean) {
        setFilterState(filter);
    }

    //#region Delete operation
    function onDeleteClicked(id: string) {
        setDialogDeleteId(id);
    }

    async function onDeleteConfirmed() {
        setIsLoading(true);
        setDialogDeleteId(undefined);
        await httpClient.delete(`${window.ENV.API_URL}/assets/${dialogDeleteId}`);
        await loadDataAsync();
    }

    function onDeleteOpenChange(_event: DialogOpenChangeEvent, _data: DialogOpenChangeData) {
        setDialogDeleteId(undefined);
    }
    //#endregion Delete operation

    return (
        <>
            <Header>
                <HeaderBar location={NavLocation.Manage} />
                <h1>{t("pages.manage.title")}</h1>
            </Header>

            <main className="grid grid-cols-1 gap-y-4 px-8 pt-8 md:grid-cols-4 md:gap-x-12 md:px-24">
                <StateFilter 
                    filterTitle={"pages.manage.filter-state"} 
                    filterOptions={"pages.manage.filter-state-"} 
                    onFilterChanged={onFilterChanged} 
                />

                <div className="col-span-3 flex flex-grow flex-wrap content-start gap-4">
                    <div className="w-full">
                        <Tooltip content={t("common.view-list")} relationship="label">
                            <ToggleButton
                                appearance="subtle"
                                icon={<TextBulletListLtrFilled />}
                                checked={viewMode === ViewMode.List}
                                onClick={() => setViewMode(ViewMode.List)}
                            />
                        </Tooltip>
                        <Tooltip content={t("common.view-cards")} relationship="label">
                            <ToggleButton
                                appearance="subtle"
                                icon={<TableSimpleFilled />}
                                checked={viewMode === ViewMode.Cards}
                                onClick={() => setViewMode(ViewMode.Cards)}
                            />
                        </Tooltip>
                        <Link to="/editor/new" tabIndex={0} className="float-right mt-auto mr-2 hover:no-underline">
                            <Button appearance="primary">{t("common.add-new")}</Button>
                        </Link>
                    </div>

                    {isLoading && (
                        <div className="mt-16 w-full">
                            <Spinner size="extra-large" />
                        </div>
                    )}
                    {!isLoading && (
                        <>
                            {queryResults.values?.length === 0 && (
                                <div className="flex w-full flex-col items-center text-center">
                                    <img src="/img/illustration-desert.svg" alt={t("pages.home.no-results")} />
                                    <div className="font-semilight text-[30px]">{t("pages.home.no-results")}</div>
                                </div>
                            )}
                            {queryResults.values?.length > 0 && viewMode === ViewMode.List && (
                                <ViewAsList items={queryResults} onDeleteClicked={onDeleteClicked} />
                            )}
                            {queryResults.values?.length > 0 && viewMode === ViewMode.Cards && (
                                <ViewAsCards items={queryResults} onDeleteClicked={onDeleteClicked} />
                            )}
                        </>
                    )}
                </div>
            </main>
            <DialogConfirm
                open={dialogDeleteId !== undefined}
                onOk={onDeleteConfirmed}
                onOpenChange={onDeleteOpenChange}
                title={t("common.confirm")}
            >
                {t("pages.manage.confirm-deletion")}
            </DialogConfirm>
        </>
    );
}
