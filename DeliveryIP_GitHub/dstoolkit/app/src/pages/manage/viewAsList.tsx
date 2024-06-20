import React, { useMemo } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Avatar, Button, Tooltip, DataGrid, DataGridBody, DataGridCell,  DataGridRow, DataGridHeader, DataGridHeaderCell, 
    TableColumnDefinition, TableCellLayout, TableRowData, DataGridProps, TableRowId , createTableColumn } from "@fluentui/react-components";
import { AssetManageItem } from "../../types/assetManageItem";
import { Paged } from "../../types/paged";
import { useTranslation } from "react-i18next";
import format from "date-fns/format";
import { CheckmarkRegular, DeleteRegular, EditRegular, History24Filled, History28Filled } from "@fluentui/react-icons";
import { isPlatformAdmin } from "../../utils/auth/roles";
import { useMsal } from "@azure/msal-react";
import { RunFlags } from "../../types/runFlags";

interface ViewAsListProps {
    items: Paged<AssetManageItem, null>;
    onDeleteClicked: (id: string) => void;
}

export function ViewAsList({ items, onDeleteClicked }: ViewAsListProps) {
    const { t } = useTranslation();
    const navigate = useNavigate();

    const { accounts } = useMsal();
    const isAdmin = isPlatformAdmin(accounts);

    const columns: TableColumnDefinition<AssetManageItem>[] = useMemo(
        () => [
            createTableColumn<AssetManageItem>({
                columnId: "name",
                compare: (a, b) => {
                    return a.name.localeCompare(b.name);
                },
                renderHeaderCell: () => {
                    return t("entities.asset.name");
                },
                renderCell: (item) => {
                    return (
                        <Link to={`/assets/${item.id}`} className="">
                            {item.name}
                        </Link>
                    );
                },
            }),            
            createTableColumn<AssetManageItem>({
                columnId: "createdOn",
                compare: (a, b) => {
                    return a.createdBy && b.createdBy ? a.createdBy.on.getTime() - b.createdBy.on.getTime() : 0;
                },
                renderHeaderCell: () => {
                    return t("entities.asset.created-on");
                },

                renderCell: (item) => {
                    return item.createdBy?.on ? format(new Date(item.createdBy.on), "yyyy-MM-dd HH:mm") : "";
                },
            }),
            createTableColumn<AssetManageItem>({
                columnId: "createdBy",
                compare: (a, b) => {
                    return a.name.localeCompare(b.name);
                },
                renderHeaderCell: () => {
                    return t("entities.asset.created-by");
                },
                renderCell: (item) => {
                    return (
                        <a href={`https://teams.microsoft.com/l/chat/0/0?users=${item.createdBy.mail}`} target="_blank">
                        <TableCellLayout
                            media={
                                <Avatar
                                    className="mr-1"
                                    size={24}
                                    name={item.createdBy?.displayName}
                                    color="colorful"
                                />
                            }
                        >
                            {item.createdBy?.displayName}
                        </TableCellLayout>
                        </a>
                    );
                },
            }),
            createTableColumn<AssetManageItem>({
                columnId: "enabled",
                compare: (a, b) => {
                    return a === b ? 0 : a ? -1 : 1;
                },
                renderHeaderCell: () => {
                    return t("entities.asset.enabled");
                },

                renderCell: (item) => {
                    return (
                        <TableCellLayout media={item.enabled ? <CheckmarkRegular /> : null}>
                            {item.enabled ? t("common.yes") : item.enabled === false ? t("common.no") : ""}
                        </TableCellLayout>
                    );
                },
            }),          
            window.ENV?.RUN_FLAGS.includes(RunFlags.WorkflowsEnabled) ?            
                createTableColumn<AssetManageItem>({
                    columnId: "deployment-history",
                    renderHeaderCell: () => {
                        return t("entities.deployment-history.deployment-history");
                    },
                    renderCell: (item) => {
                        return (
                            <Tooltip content={t("entities.deployment-history.deployment-history")} relationship="label">
                                <Link to={`/deployment-history/${item.id}`} tabIndex={0} className="hover:no-underline" >
                                    <Button 
                                        className="mr-2"
                                        appearance="subtle"
                                        icon={<History28Filled />}
                                    />
                                </Link>
                            </Tooltip> 
                        );
                    },
                })
                : createTableColumn<AssetManageItem>({
                    columnId: "deployment-history",
                }),            
            createTableColumn<AssetManageItem>({
                columnId: "operations",

                renderHeaderCell: () => null,

                renderCell: (item) => {
                    return (
                        <span>
                            <Tooltip content={t("common.edit")} relationship="label">
                                <Link to={`/editor/${item.id}`} className="mt-auto hover:no-underline">
                                    <Button className="mr-2" appearance="subtle" icon={<EditRegular />} />
                                </Link>
                            </Tooltip>
                            { !item.enabled && isAdmin ? (
                                <Tooltip content={t("common.delete")} relationship="label">
                                    <Button
                                        className="mr-2"
                                        appearance="subtle"
                                        icon={<DeleteRegular />}
                                        onClick={() => onDeleteClicked(item.id)}
                                    />
                                </Tooltip>
                            ) : null }
                        </span>
                    );
                },
            })
        ],
        []
    );

    const [selectedRows, setSelectedRows] = React.useState(new Set<TableRowId>([]));
    const onSelectionChange: DataGridProps["onSelectionChange"] = (e, data) => {
        setSelectedRows(data.selectedItems);
    };

    return (
        <DataGrid
            items={items.values}
            columns={columns}
            selectionMode={undefined}
            selectedItems={selectedRows}
            onSelectionChange={onSelectionChange}
            subtleSelection
            className="w-full"
        >
            <DataGridHeader>
                <DataGridRow>
                    {({ renderHeaderCell }: TableColumnDefinition<AssetManageItem>) => (
                        <DataGridHeaderCell>{renderHeaderCell()}</DataGridHeaderCell>
                    )}
                </DataGridRow>
            </DataGridHeader>
            <DataGridBody>
                {({ item, rowId }: TableRowData<AssetManageItem>) => (
                    <DataGridRow key={rowId}>
                        {({ renderCell }: TableColumnDefinition<AssetManageItem>) => (
                            <DataGridCell>{renderCell(item)}</DataGridCell>
                        )}
                    </DataGridRow>
                )}
            </DataGridBody>
        </DataGrid>
    );
}
