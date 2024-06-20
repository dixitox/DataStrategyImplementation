import React, { MouseEventHandler } from "react";
import { Avatar, Button, Tooltip } from "@fluentui/react-components";
import format from "date-fns/format";
import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";
import { AssetListItem } from "../../types/assetListItem";
import { Counter } from "../counter/counter";
import { getIndustriesColors, getIndustryColor } from "../../utils/industryColors/industryColors";
import { DeleteRegular, EditRegular } from "@fluentui/react-icons";

interface AssetCardProps {
    asset: AssetListItem;
    editEnabled?: boolean;
    deleteEnabled?: boolean;
    onDeleteClicked?: MouseEventHandler<HTMLElement>;
}

export function AssetCard({ asset: item, editEnabled, deleteEnabled, onDeleteClicked }: AssetCardProps) {
    const { t } = useTranslation();

    return (
        <div className="min-h-80 flex w-64 min-w-[278px] flex-grow flex-col overflow-hidden rounded-b-xl bg-white py-5 pl-5 pr-4 shadow-lg">
            <div className="-mt-5 -mr-4 -ml-5 h-1" style={{ background: getIndustriesColors(item.industries) }} />
            <div
                className="my-4 flex h-7 items-center text-sm font-bold uppercase tracking-wider"
                style={{ color: getIndustryColor(item.industries[0]) }}
            >
                {item.industries[0]}
                {item.industries.length > 1 && (
                    <Tooltip content={item.industries.slice(1).join(", ")} relationship="description">
                        <span>
                            <Counter className="ml-2" value={`+${item.industries.length - 1}`} />
                        </span>
                    </Tooltip>
                )}
            </div>
            <h6 className="border-b border-b-neutral-300 pb-5">{item.name}</h6>
            <p className="text-clamp-4 min-h-[60px] text-ellipsis pt-5">{item.tagline}</p>

            <div className="flex items-center pt-5 text-[12px]">
                {t("components.asset-card.created")}:&nbsp;
                <span className="whitespace-nowrap font-semibold">
                    {format(
                        item.releasedOn ? new Date(item.releasedOn) : new Date("2022-01-01T00:00Z"),
                        "MMM. d, yyyy"
                    )}
                </span>
                {item.authors.length > 0 && (
                    <>
                        <span className="mx-2 font-normal text-black">{t("components.asset-card.by")}</span>
                        {item.authors.map((author, idx) =>
                            idx < 3 || item.authors.length <= 4 ? (
                                <Tooltip
                                    key={idx}
                                    content={author.name || author.gitHubAlias || "-"}
                                    relationship="description"
                                >
                                    <Avatar
                                        className="mr-1"
                                        size={24}
                                        name={author.name || author.gitHubAlias}
                                        color="colorful"
                                        image={author.gitHubAvatar ? { src: author.gitHubAvatar } : undefined}
                                    />
                                </Tooltip>
                            ) : null
                        )}
                        {item.authors.length > 4 && (
                            <Tooltip
                                content={item.authors
                                    .slice(3)
                                    .map((o) => o.name || o.gitHubAlias)
                                    .join(", ")}
                                relationship="description"
                            >
                                <span>
                                    <Counter className="mr-1" value={`+${item.authors.length - 3}`} />
                                </span>
                            </Tooltip>
                        )}
                        {item.authors.length === 1 && (
                            <span className="text-clamp-1 ml-1 font-normal text-neutral-700">
                                {item.authors[0].name}
                            </span>
                        )}
                    </>
                )}
            </div>
            <p className="my-5 text-[12px]">
                <span>{t("entities.asset.asset-type")}:&nbsp;</span>
                <span className="font-semibold">
                    {item.assetType.map((type, idx) => `${idx > 0 ? ", " : ""}${type}`)}
                </span>
            </p>
            <div>
                <Link to={`/assets/${item.id}`} tabIndex={0} className="mt-auto mr-2 hover:no-underline">
                    <Button appearance="primary">{t("common.see-more")}</Button>
                </Link>
                {editEnabled && (
                    <Tooltip content={t("common.edit")} relationship="label">
                        <Link to={`/editor/${item.id}`} tabIndex={0} className="mt-auto mr-2 hover:no-underline">
                            <Button appearance="subtle" icon={<EditRegular />} />
                        </Link>
                    </Tooltip>
                )}
                {deleteEnabled && (
                    <Tooltip content={t("common.delete")} relationship="label">
                        <Button appearance="subtle" icon={<DeleteRegular />} onClick={onDeleteClicked} />
                    </Tooltip>
                )}
            </div>
        </div>
    );
}
