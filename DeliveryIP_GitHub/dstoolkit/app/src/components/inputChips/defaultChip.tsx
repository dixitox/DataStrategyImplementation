import React, { FC } from "react";
import { DismissRegular } from "@fluentui/react-icons";
import { RemovableChipData } from "./types";

interface Props {
    value: RemovableChipData;
}

const DefaultChip: FC<Props> = ({ value }) => {
    const removeThisChip = () => value.onRemove(value);
    return (
        <div className="chip">
            {value.name}
            <div className="remove" onClick={removeThisChip}>
                <DismissRegular />
            </div>
        </div>
    );
};

export default DefaultChip;
