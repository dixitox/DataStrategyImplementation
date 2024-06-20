import React from "react";
import { Avatar, Button, Tooltip } from "@fluentui/react-components";
import { useTranslation } from "react-i18next";
import { Link, useNavigate } from "react-router-dom";
import { useMsal } from "@azure/msal-react";
import { EditRegular, DeleteRegular } from "@fluentui/react-icons";
import format from "date-fns/format";
import { isPlatformAdmin } from "../../utils/auth/roles";
import { BlogEntry } from "../../types/blogEntry";
import { BlogEntryStatus } from "../../types/blogEntryStatus";

interface BlogItemRenderProps {
    id?: string;
    item: BlogEntry;
    showFull: boolean;
    onDeleteClicked?: (id: string) => void;
}

export function BlogItemRender({ id, item, showFull, onDeleteClicked }: BlogItemRenderProps) {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const { accounts } = useMsal();

    function canEdit(): boolean {
        if (!accounts?.length) return false;
        return isPlatformAdmin(accounts);
    }

    // The "-mt-5 pt-5" allows the element anchor navigation to leave a padding at the top
    return (
        <div id={id || "blog-item"} className="-mt-5 pt-5">
            <div className="mb-7 flex flex-grow flex-col rounded-b-xl bg-white px-7 pt-5 pb-7 shadow-lg">
                <div
                    className="-mx-7 -mt-5 h-1"
                    style={
                        canEdit() && item.status && item.status !== BlogEntryStatus.Published
                            ? {
                                  backgroundImage:
                                      "repeating-linear-gradient(-45deg, #D5D5D5, #D5D5D5 0.99%, #F5F5F5 1%, #F5F5F5 2%)",
                              }
                            : { background: "#D5D5D5" }
                    }
                />

                <div className="text-neutral-550 flex flex-wrap items-center gap-y-3 pt-6 text-sm font-bold uppercase tracking-wider">
                    {item.entryType}
                    {canEdit() && item.status && item.status !== BlogEntryStatus.Published && (
                        <div className="text-primary-100 ml-4 font-bold">
                            {t(`entities.blog.status-${item.status.toLocaleLowerCase()}`)}
                        </div>
                    )}
                    {canEdit() && (
                        <div className="ml-auto flex gap-x-3">
                            {onDeleteClicked && item.status === BlogEntryStatus.Draft && (
                                <Button
                                    appearance="secondary"
                                    icon={<DeleteRegular />}
                                    onClick={() => onDeleteClicked(item.id)}
                                >
                                    {t("common.delete")}
                                </Button>
                            )}

                            <Button
                                appearance="secondary"
                                icon={<EditRegular />}
                                onClick={() => navigate(`/blog-editor/${item.id}`)}
                            >
                                {t("common.edit")}
                            </Button>
                        </div>
                    )}
                </div>

                <Link to={`/blog/${item.id}`} className={showFull ? "pointer-events-none" : "pointer-events-auto"}>
                    <h4 className="py-3">{item.title}</h4>
                </Link>

               


                <div className="mt-3 flex items-center border-y border-y-neutral-300 py-5 text-[12px] text-black">
                    {t("entities.blog.created")}:&nbsp;
                    <span className="whitespace-nowrap font-semibold">
                        {format(
                            item.createdBy?.on ? new Date(item.createdBy?.on) : new Date("2022-01-01T00:00Z"),
                            "MMM. d, yyyy"
                        )}
                    </span>
                    <span className="mx-2">|</span>


            <span className="mr-2">{t("entities.blog.author", { count: item.authors.length })}:</span> 
            {item.authors.map((author, idx) => (
                <Tooltip key={idx} content={author.name || author.gitHubAlias || "-"} relationship="label">         

                {author.linkedin ? (        
                    <a
                        href={`${author.linkedin}`}
                        target="_blank"
                        rel="noopener noreferrer"
                    >
                        <Avatar
                            className="mr-1"
                            size={24}
                            name={author.name}
                            color="colorful"
                            image={author.gitHubAvatar ? { src: author.gitHubAvatar } : undefined}
                        />
                    </a>
                ) : (
                    <Avatar
                    className="mr-1"
                    size={24}
                    name={author.name}
                    color="colorful"
                    image={author.gitHubAvatar ? { src: author.gitHubAvatar } : undefined}
                />
                )}
                </Tooltip>
            ))}

            {item.authors.length === 0 &&  (
                <>
                    <Avatar className="mr-1" size={24} name={item.createdBy?.displayName} color="colorful" />            
                    <span className="ml-1 font-normal text-neutral-700">{item.createdBy?.displayName}</span>
                </>
            )}

            {item.authors.length === 1 && (
                <span className="ml-1 font-normal text-neutral-700">{item.authors[0].name}</span>
            )}


                   
                </div>

                {!showFull && ( 
                    <div className="mt-5">{item.introduction}</div>
                )}

                {item.heroImage?.path && (
                    <Link to={`/blog/${item.id}`} className={showFull ? "pointer-events-none" : "pointer-events-auto"}>
                        <img
                            src={`${window.ENV.IMG_URL}/${item.heroImage.path}`}
                            alt={item.heroImage.alternativeText || item.title}
                            className="m-auto mt-5 h-auto w-full rounded-xl object-cover"
                        />
                    </Link>
                )}
                {showFull && item.body && (
                    <div
                        className="mt-4"
                        dangerouslySetInnerHTML={{
                            __html: item.body,
                        }}
                    />
                )}
            </div>
        </div>
    );
}
