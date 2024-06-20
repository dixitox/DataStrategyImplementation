import React from "react";
import { AssetCard } from "../../components/assetCard/assetCard";
import { AssetManageItem } from "../../types/assetManageItem";
import { Paged } from "../../types/paged";

interface ViewAsCardsProps {
    items: Paged<AssetManageItem, null>;
    onDeleteClicked: (id: string) => void;
}

export function ViewAsCards({ items, onDeleteClicked }: ViewAsCardsProps) {
    return (
        <>
            {items.values?.map((item, idx) => (
                <AssetCard
                    key={idx}
                    asset={item}
                    editEnabled={true}
                    deleteEnabled={!item.enabled}
                    onDeleteClicked={() => onDeleteClicked(item.id)}
                />
            ))}
            {/* Add fake items so that items on last row don't get bigger than the others */}
            {[...Array(4).keys()].map((item) => (
                <div key={"fake" + item} className="h-4 w-64 flex-grow bg-transparent"></div>
            ))}
        </>
    );
}
