import React, { useState } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { httpClient } from "../../utils/httpClient/httpClient";
import { useEffectOnce } from "../../utils/react/useEffectOnce";
import { useMsal } from "@azure/msal-react";
import { Button, Spinner } from "@fluentui/react-components";
import { HeaderBar } from "../../components/headerBar/headerBar";
import { Header } from "../../components/header/header";
import { AddRegular } from "@fluentui/react-icons";
import { BlogEntryType } from "../../types/blogEntryType";
import { isPlatformAdmin } from "../../utils/auth/roles";
import { Paged } from "../../types/paged";
import { BlogEntry } from "../../types/blogEntry";
import { BlogItemRender } from "../blogViewer/blogItemRender";
import { DialogConfirm } from "../../components/dialogConfirm/dialogConfirm";

export function BlogList() {
    const { t } = useTranslation();
    const { accounts } = useMsal();
    const [blogItems, setBlogItems] = useState<Paged<BlogEntry>>();
    const [firstOfType, setFirstOfType] = useState<Record<string, number>>();
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const [dialogDeleteId, setDialogDeleteId] = useState<string | undefined>(undefined);
    const navigate = useNavigate();
    const idPrefix: string = "blog-";

    // Custom hook that can be used instead of useEffect() with zero dependencies.
    // Avoids a double execution of the effect when in React 18 DEV mode with <React.StrictMode>
    useEffectOnce(() => {
        loadDataAsync();
    });

    async function loadDataAsync() {
        setIsLoading(true);
        const res: Paged<BlogEntry> = await httpClient.get(`${window.ENV.API_URL}/blogentries`);
        setBlogItems(res);
        setFirstOfType({
            [BlogEntryType.Announcement]: res.values.findIndex((o) => o.entryType === BlogEntryType.Announcement),
            [BlogEntryType.Guidance]: res.values.findIndex((o) => o.entryType === BlogEntryType.Guidance),
        });
        setIsLoading(false);
    }

    function canEdit(): boolean {
        if (!accounts?.length) return false;
        return isPlatformAdmin(accounts);
    }

    //#region Delete operation
    function onDeleteClicked(id: string) {
        setDialogDeleteId(id);
    }

    async function onDeleteConfirmed() {
        setIsLoading(true);
        setDialogDeleteId(undefined);
        await httpClient.delete(`${window.ENV.API_URL}/blogentries/${dialogDeleteId}`);
        await loadDataAsync();
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
                        {t("pages.blog-list.title")}
                    </div>
                    <div className="text-base font-semibold leading-8">
                        <a href={`#${idPrefix}0`}>
                            <div>{t("common.all")}</div>
                        </a>
                        {firstOfType && firstOfType[BlogEntryType.Announcement] >= 0 && (
                            <a href={`#${idPrefix}${firstOfType[BlogEntryType.Announcement]}`}>
                                <div>
                                    {t(`entities.blog.entrytype-${BlogEntryType.Announcement.toLocaleLowerCase()}`)}
                                </div>
                            </a>
                        )}
                        {firstOfType && firstOfType[BlogEntryType.Guidance] >= 0 && (
                            <a href={`#${idPrefix}${firstOfType[BlogEntryType.Guidance]}`}>
                                <div>{t(`entities.blog.entrytype-${BlogEntryType.Guidance.toLocaleLowerCase()}`)}</div>
                            </a>
                        )}
                    </div>
                </div>

                <div className="col-span-3">
                    {isLoading && (
                        <div className="mt-16">
                            <Spinner size="extra-large" />
                        </div>
                    )}

                    {!isLoading && canEdit() && (
                        <div className="mt-[-3.25rem] mb-5 flex h-8 flex-wrap gap-2.5">
                            <Button
                                appearance="secondary"
                                icon={<AddRegular />}
                                onClick={() => navigate(`/blog-editor`)}
                            >
                                {t("common.add")}
                            </Button>
                        </div>
                    )}
                    {!isLoading &&
                        blogItems &&
                        blogItems.values.map((value, index, _array) => (
                            <BlogItemRender
                                key={value.id}
                                id={`${idPrefix}${index}`}
                                item={value}
                                showFull={false}
                                onDeleteClicked={onDeleteClicked}
                            />
                        ))}
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
