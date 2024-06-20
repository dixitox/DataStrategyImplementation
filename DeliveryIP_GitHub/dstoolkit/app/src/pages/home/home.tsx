import React, { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { HeaderBar, NavLocation } from "../../components/headerBar/headerBar";
import { AssetCard } from "../../components/assetCard/assetCard";
import { Paged } from "../../types/paged";
import { Spinner, Dropdown, Option } from "@fluentui/react-components";
import { AssetListItem } from "../../types/assetListItem";
import { httpClient } from "../../utils/httpClient/httpClient";
import { Filter } from "../../components/filter/filter";
import { Facet, FacetType } from "../../types/facet";
import { SearchBox, SearchBoxHandle } from "../../components/searchBox/searchBox";
import { useNavigate, useSearchParams } from "react-router-dom";
import { SearchRequest } from "../../types/searchRequest";
import { SortCriteria } from "../../types/sortCriteria";
import { Header } from "../../components/header/header";
import { RunFlags } from "../../types/runFlags";

interface HomeProps {
    isSearchResultsPage?: boolean;
}

export function Home({ isSearchResultsPage }: HomeProps) {
    const { t } = useTranslation();
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const [filters, setFilters] = useState<Record<FacetType, string[]>>();
    const [queryResults, setQueryResults] = useState<Paged<AssetListItem, Facet[]>>(
        {} as Paged<AssetListItem, Facet[]>
    );
    const [query, setQuery] = useState<string>();
    const [sort, setSort] = useState<SortCriteria | undefined>(undefined);
    const searchBoxRef = React.createRef<SearchBoxHandle>();
    const [searchParams, setSearchParams] = useSearchParams();
    const runFlagNoFilters = window.ENV?.RUN_FLAGS.includes(RunFlags.NoFilters);
    const navigate = useNavigate();

    // Custom hook that can be used instead of useEffect() with zero dependencies.
    // Avoids a double execution of the effect when in React 18 DEV mode with <React.StrictMode>
    // useEffectOnce(() => {
    // });

    useEffect(() => {
        // console.log("*** isSearchResultsPage", isSearchResultsPage);
        if (isSearchResultsPage) {
            setQuery(decodeURIComponent(searchParams.get("q") || ""));
        } else if (query) {
            // We are back to home - clear query
            setQuery("");
            searchBoxRef.current?.reset();
        }
    }, [isSearchResultsPage]);

    useEffect(() => {
        // console.log("*** query", query);
        // Wait till query is defined unless this is the initial hit to home page
        if (!isSearchResultsPage || query !== undefined) loadDataAsync();
    }, [query]);

    useEffect(() => {
        // console.log("*** filters", filters);
        // Don't load data again on startup, only load if filters are set
        if (filters) loadDataAsync();
    }, [filters]);

    useEffect(() => {
        // Don't load data again on startup, only load if sort is set
        if (sort) loadDataAsync();
    }, [sort]);

    async function loadDataAsync() {
        // console.log("*** loadData", "query", query, "filters", filters);

        setIsLoading(true);

        const payload: SearchRequest = {
            search: query || "",
            filters: {
                industry: filters?.industries || [],
                tags: filters?.tags || [],
                assetType: filters?.assetType || [],
            },
            sortBy: sort || SortCriteria.Alphabetical,
            pageSize: 100,
            page: 1,
        };

        const result: Paged<AssetListItem, Facet[]> = await httpClient.post(
            `${window.ENV.API_URL}/assets/search`,
            payload
        );

        setQueryResults(result);
        setIsLoading(false);
    }

    function onFilterChanged(newFilters: Record<FacetType, string[]>): void {
        setFilters(newFilters);
    }

    function onSearchChanged(searchValue: string): void {
        if (searchValue) {
            if (isSearchResultsPage) {
                const updatedSearchParams = new URLSearchParams(searchParams.toString());
                updatedSearchParams.set("q", encodeURIComponent(searchValue));
                setSearchParams(updatedSearchParams.toString());
                setQuery(searchValue);
            } else {
                navigate(`/search?q=${encodeURIComponent(searchValue)}`);
            }
        } else {
            setSearchParams("");
            setQuery("");
        }
    }

    function onSortSelect(_event: unknown, data: { optionValue?: string }) {
        setSort(data.optionValue as SortCriteria | undefined);
    }

    return (
        <>
            <Header
                className="bg-contain bg-right-bottom bg-no-repeat md:bg-[url('/img/header-default.png')]"
                size={!isSearchResultsPage ? "large" : "medium"}
            >
                <HeaderBar location={isSearchResultsPage ? NavLocation.Assets : NavLocation.Home} />
                <div>
                    {!isSearchResultsPage && (
                        <>
                            <h1 className="max-sm:text-3xl">{t("pages.home.title")}</h1>
                            <div className="mb-7 w-full text-lg md:w-1/2">{t("pages.home.subtitle")}</div>
                        </>
                    )}
                    <SearchBox
                        ref={searchBoxRef}
                        className={`w-full ${
                            !isSearchResultsPage ? "items-center" : "items-baseline justify-center max-sm:items-center"
                        }`}
                        labelClassName={`font-semilight ${
                            !isSearchResultsPage
                                ? "text-[23px] max-sm:text-base"
                                : "text-[33px] max-sm:text-base leading-8"
                        }`}
                        inputClassName="max-w-xs flex-grow"
                        onSearchChanged={onSearchChanged}
                        initialValue={query}
                    />
                </div>
            </Header>
            <main className="grid grid-cols-1 gap-y-4 px-8 pt-8 md:grid-cols-4 md:gap-x-12 md:px-24">
                {!runFlagNoFilters && (
                    <Filter className="mt-5" facetsCounts={queryResults.facets} onFilterChanged={onFilterChanged} />
                )}

                <div
                    className={`${
                        runFlagNoFilters ? "col-span-4" : "col-span-3"
                    } flex flex-grow flex-wrap content-start gap-4`}
                >
                    <div className="flex w-full items-center justify-end">
                        <div className="pr-2 pt-1.5 text-[12px] font-semibold">{t("common.sort-by")}</div>
                        <Dropdown title={t("common.sort-by")}
                            onOptionSelect={onSortSelect}
                            selectedOptions={[sort || SortCriteria.Alphabetical]}
                            value={t(
                                `entities.sort-criteria.${(sort || SortCriteria.Alphabetical).toLocaleLowerCase()}`
                            )}
                        >
                            {/* Relevance criteria is only available if there is a query (or it is the currently selected one) */}
                            {Object.values(SortCriteria)
                                .filter((o) => o !== SortCriteria.Relevance || query || sort == SortCriteria.Relevance)
                                .map((o) => (
                                    <Option
                                        key={o}
                                        value={o}
                                        text={t(`entities.sort-criteria.${o.toLocaleLowerCase()}`)!} // eslint-disable-line @typescript-eslint/no-non-null-assertion
                                    >
                                        {t(`entities.sort-criteria.${o.toLocaleLowerCase()}`)}
                                    </Option>
                                ))}
                        </Dropdown>
                    </div>
                    {isLoading && (
                        <div className="mt-16 w-full">
                            <Spinner size="extra-large" />
                        </div>
                    )}
                    {!isLoading && (
                        <>
                            {queryResults.totalResults === 0 && (
                                <div className="flex w-full flex-col items-center text-center">
                                    <img src="/img/illustration-desert.svg" alt={t("pages.home.no-results")} />
                                    <div className="font-semilight text-[30px]">{t("pages.home.no-results")}</div>
                                    {query && (
                                        <>
                                            <div className="mt-6">
                                                {t("pages.home.no-results-no-match", { query: query })}
                                            </div>
                                            <div>{t("pages.home.no-results-footer")}</div>
                                        </>
                                    )}
                                </div>
                            )}
                            {query && queryResults.totalResults > 0 && (
                                <div className="w-full text-xl">
                                    {t("pages.home.search-results", { count: queryResults.totalResults })}
                                    <span className="mx-2 font-semibold">&quot;{query}&quot;</span>
                                    {t("pages.home.search-in")}
                                </div>
                            )}

                            {queryResults.totalResults > 0 && (
                                <>
                                    {queryResults.values?.map((item, idx) => (
                                        <AssetCard key={idx} asset={item} />
                                    ))}
                                    {/* Add fake items so that items on last row don't get bigger than the others */}
                                    {[...Array(4).keys()].map((item) => (
                                        <div key={"fake" + item} className="h-4 w-64 flex-grow bg-transparent"></div>
                                    ))}
                                </>
                            )}
                        </>
                    )}
                </div>
            </main>
        </>
    );
}
