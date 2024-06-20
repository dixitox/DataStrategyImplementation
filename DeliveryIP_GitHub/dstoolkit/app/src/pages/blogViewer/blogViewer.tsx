import React, { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router-dom";
import { httpClient } from "../../utils/httpClient/httpClient";
import { useEffectOnce } from "../../utils/react/useEffectOnce";
import { Spinner } from "@fluentui/react-components";
import { HeaderBar } from "../../components/headerBar/headerBar";
import { Header } from "../../components/header/header";
import { BlogEntry } from "../../types/blogEntry";
import { BlogItemRender } from "./blogItemRender";
import { DialogConfirm } from "../../components/dialogConfirm/dialogConfirm";

export function BlogViewer() {
    const { t } = useTranslation();
    const params = useParams();
    const [data, setData] = useState<BlogEntry>();
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const [dialogDeleteId, setDialogDeleteId] = useState<string | undefined>(undefined);
    const navigate = useNavigate();

    // Custom hook that can be used instead of useEffect() with zero dependencies.
    // Avoids a double execution of the effect when in React 18 DEV mode with <React.StrictMode>
    useEffectOnce(() => {
        loadDataAsync(params.blogId!);
    });

    useEffect(() => {
        // Restore original page title on exit
        return () => {
            document.title = t("common.title");
        };
    }, []);

    async function loadDataAsync(blogId: string) {
        setIsLoading(true);
        const res: BlogEntry = await httpClient.get(`${window.ENV.API_URL}/blogentries/${blogId}`);
        document.title = res.title;
        setData(res);
        setIsLoading(false);
    }

    //#region Delete operation
    function onDeleteClicked(id: string) {
        setDialogDeleteId(id);
    }

    async function onDeleteConfirmed() {
        setIsLoading(true);
        setDialogDeleteId(undefined);
        await httpClient.delete(`${window.ENV.API_URL}/blogentries/${dialogDeleteId}`);
        navigate("/blog");
    }

    function onDeleteOpenChange(_event: unknown, _data: unknown) {
        setDialogDeleteId(undefined);
    }
    //#endregion Delete operation

    return (
        <>
            <Header>
                <HeaderBar />
            </Header>

            <main className="grid grid-cols-1 gap-4 px-8 md:grid-cols-4 md:gap-x-12 md:px-24">
                {/* Left nav bar */}
                <div className="whitespace-nowrap md:sticky md:top-5 md:self-start">
                    <div className="w-fit border-t-4 py-6 text-sm font-bold uppercase">
                        {t("pages.blog-viewer.title-left")}
                    </div>
                </div>

                <div className="col-span-3">
                    {isLoading && (
                        <div className="mt-16">
                            <Spinner size="extra-large" />
                        </div>
                    )}
                    {!isLoading && data && (
                        <BlogItemRender item={data} showFull={true} onDeleteClicked={onDeleteClicked} />
                    )}
                </div>
            </main>
            <DialogConfirm
                open={dialogDeleteId !== undefined}
                onOk={onDeleteConfirmed}
                onOpenChange={onDeleteOpenChange}
                title={t("common.confirm")}
            >
                {t("pages.blog-list.confirm-deletion")}
            </DialogConfirm>
        </>
    );
}
