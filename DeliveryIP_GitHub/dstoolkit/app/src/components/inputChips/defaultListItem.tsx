import React, { FC } from "react";

import { ChipData } from "./types";

interface Props {
    value: ChipData;
    selected: boolean;
    handleSelect: (val: ChipData) => void;
    preSelected: boolean;
}

const DefaultListItem: FC<Props> = ({ value, handleSelect, selected, preSelected }) => {
    const selectItem = () => handleSelect(value);
    return (
        <div
            className={`list_item ${preSelected ? "pre_selected" : ""} ${selected ? "selected" : ""}`}
            onClick={selectItem}
        >
            {value.name}
        </div>
    );
};

export default DefaultListItem;
