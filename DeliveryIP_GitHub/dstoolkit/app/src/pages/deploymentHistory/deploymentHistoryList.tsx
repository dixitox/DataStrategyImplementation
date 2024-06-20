import { Avatar, createTableColumn, DataGrid, DataGridBody, DataGridCell, DataGridHeader, DataGridHeaderCell, DataGridProps, DataGridRow, TableCellLayout, TableColumnDefinition, TableRowData, TableRowId } from '@fluentui/react-components';
import format from 'date-fns/format';
import React, { useEffect, useMemo } from 'react'
import { useTranslation } from 'react-i18next';
import { DeploymentHistoryListItem } from '../../types/deploymentHistoryListItem';
import { GitHubIcon } from '../../assets/icons/gitHubLogoIcon';

interface DeploymentHistoryListProps {
    items: DeploymentHistoryListItem[];
}

export function DeploymentHistoryList({items}: DeploymentHistoryListProps) {
    
    const {t} = useTranslation();    

    const columns: TableColumnDefinition<DeploymentHistoryListItem>[] = useMemo(
        () => [
            createTableColumn<DeploymentHistoryListItem>({
                columnId: "environment",
                compare: (a, b) => {
                    return a.environment.localeCompare(b.environment)
                },

                renderHeaderCell: () => {
                    return t("entities.deployment-history.environment")
                },
                renderCell: (item) => {
                    return item.environment;
                },
            }),
            createTableColumn<DeploymentHistoryListItem>({
                columnId: "runId",
                compare: (a, b) => {
                    return a.runId.localeCompare(b.runId)
                },
                renderHeaderCell: () => {
                    return t("entities.deployment-history.runId")
                },
                renderCell: (item) => {
                    return (
                        <a href={`${item.repositoryUrl}/actions/runs/${item.runId}`} target="_blank" rel="noopener noreferrer">
                        <TableCellLayout
                            media={
                                <GitHubIcon className="h-4 w-4 fill-black" />
                            }
                        >
                            {item.runId}
                        </TableCellLayout>
                        </a>
                    )
                },
            }),
            createTableColumn<DeploymentHistoryListItem>({
                columnId: "lastUpdatedOn",
                compare: (a, b) => {
                    return a.lastUpdatedBy.on && b.lastUpdatedBy.on ? a.lastUpdatedBy.on.getTime() - b.lastUpdatedBy.on.getTime() : 0;
                },
                renderHeaderCell: () => {
                    return t("entities.deployment-history.updatedOn")
                },
                renderCell: (item) => {
                    return item.lastUpdatedBy.on? format(new Date(item.lastUpdatedBy.on), "yyyy-MM-dd HH:mm") : "";
                },
            }),
            createTableColumn<DeploymentHistoryListItem>({
                columnId: "LastUpdatedBy",
                compare: (a, b) => {
                    return a.lastUpdatedBy.displayName.localeCompare(b.lastUpdatedBy.displayName)
                },
                renderHeaderCell: () => {
                    return t("entities.deployment-history.updatedBy")
                },
                renderCell: (item) => {
                    return (
                        <a href={`https://teams.microsoft.com/l/chat/0/0?users=${item.lastUpdatedBy.mail}`} target="_blank">
                        <TableCellLayout
                            media={
                                <Avatar
                                    className="mr-1"
                                    size={24}
                                    name={item.lastUpdatedBy.displayName}
                                    color="colorful"
                                />
                            }
                        >
                            {item.lastUpdatedBy.displayName}
                        </TableCellLayout>
                        </a>
                    );
                },
            }),
        ],
        []
    );

    const [selectedRows, setSelectedRows] = React.useState(new Set<TableRowId>([]));
    const onSelectionChange: DataGridProps["onSelectionChange"] = (e, data) => {
        setSelectedRows(data.selectedItems);
    }
                    

    



    return (
        <DataGrid
            items={items}
            columns={columns}
            selectionMode={undefined}
            selectedItems={selectedRows}
            onSelectionChange={onSelectionChange}
            subtleSelection
            className="w-full"
        >
            <DataGridHeader>
                <DataGridRow>
                    {({ renderHeaderCell }: TableColumnDefinition<DeploymentHistoryListItem>) => (
                        <DataGridHeaderCell>{renderHeaderCell()}</DataGridHeaderCell>
                    )}
                </DataGridRow>
            </DataGridHeader>
            <DataGridBody>
                {({ item, rowId }: TableRowData<DeploymentHistoryListItem>) => (
                    <DataGridRow key={rowId}>
                        {({ renderCell }: TableColumnDefinition<DeploymentHistoryListItem>) => (
                            <DataGridCell>{renderCell(item)}</DataGridCell>
                        )}
                    </DataGridRow>
                )}
            </DataGridBody>
        </DataGrid>
    );
}
