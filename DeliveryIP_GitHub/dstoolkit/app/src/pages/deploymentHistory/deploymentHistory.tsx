import { Spinner } from "@fluentui/react-components";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useParams } from "react-router-dom";
import { Header } from "../../components/header/header";
import { HeaderBar, NavLocation } from "../../components/headerBar/headerBar";
import { DeploymentHistoryListItem } from "../../types/deploymentHistoryListItem";
import { httpClient } from "../../utils/httpClient/httpClient";
import { useEffectOnce } from "../../utils/react/useEffectOnce";
import { StateFilter } from "../manage/stateFilter";
import { DeploymentHistoryList } from "./deploymentHistoryList";
import { AssetItem } from "../../types/assetItem";
import { AssetSummary } from "../asset/assetSummary";



export function DeploymentHistory() {

    const { t } = useTranslation();
    
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const [isLoadingAsset, setIsLoadingAsset] = useState<boolean>(false);
    const [filterState, setFilterState] = useState<string | undefined>("");
    const [results, setResults] = useState<DeploymentHistoryListItem[]>([]);
    const [asset, setAsset] = useState<AssetItem>();

    const { assetId } = useParams<{ assetId: string}>();

    async function loadAssetSummary(assetId: string){
        setIsLoading(true);
        
        const res: AssetItem = await httpClient.get(`${window.ENV.API_URL}/assets/${assetId}`);
        setAsset(res);
        setIsLoadingAsset(false);
        
    }

    async function loadDataAsync() {
        setIsLoading(true);

        try{
            const baseURL = `${window.ENV.API_URL}/Asset/${assetId}/DeploymentHistory`;
            const url = filterState ? `${baseURL}?environments=${filterState}` : baseURL;

        const result: DeploymentHistoryListItem[] = await httpClient.get(
            url
        );

        setResults(result);
        } catch (error) {
            console.log("error", error);
        } finally {
            setIsLoading(false);
        }
    }

    useEffectOnce(() => {
        loadAssetSummary(`${assetId}`);

    });



    useEffect(() => {
        console.log("filterState", filterState);

        loadDataAsync();
        console.log("Call to API to get Filtered Results");

        
    }, [filterState]);

    useEffect(() => {
        if (results) {
            console.log("results received: ", results);
        } else {
            console.log("no results");
        }
        
    }, [results]);


    function onFilterChanged(filter?: boolean) {

        switch (filter) {
            case true:
                setFilterState("Sandbox");
                break;
            case false:
                setFilterState("Production");
                break;
            default:
                setFilterState("");
                break;
        }
    }


    return (
        <>
            <Header>
                <HeaderBar location={NavLocation.DeploymentHistory} />
                <h1>{t("pages.deployment-history.title")}</h1>
            </Header>

            <main className="grid grid-cols-1 gap-y-4 px-8 pt-8 md:grid-cols-4 md:gap-x-12 md:px-24">

                <StateFilter 
                    filterTitle={t("pages.deployment-history.filter-environment")} 
                    filterOptions={"pages.deployment-history.filter-environment-"} 
                    onFilterChanged={onFilterChanged} 
                />

                <div className="col-span-3 flex flex-grow flex-wrap content-start gap-4">

                    {isLoadingAsset && (
                        <div className="mt-16 w-full">
                            <Spinner size="extra-large" />
                        </div>
                    )}

                    {!isLoadingAsset && asset && (
                        <>
                            <AssetSummary asset={asset} />
                        </>
                        
                    )}

                    {isLoading && (
                        <div className="mt-16 w-full">
                            <Spinner size="extra-large" />
                        </div>
                    )}

                    {!isLoading && (
                        <>
                            {results?.length === 0 && (
                                <div className="flex w-full flex-col items-center text-center">
                                    <img src="/img/illustration-desert.svg" alt={t("pages.deployment-history.no-deployments-found")} />
                                <div className="font-semilight text-[30px]">{t("pages.deployment-history.no-deployments-found")}</div>
                            </div>
                            )}
                            {results.length != 0 && (
                                <>
                                    <DeploymentHistoryList items={results} />
                                </>
                            )}
                        </>
                    )}

                </div>

            </main>



        </>

    );

}

