// Adapted from
// https://github.com/loweisz/react-custom-chips/blob/master/src/CustomChips.tsx

import React, { FC, useEffect, useRef, useState } from "react";
import SearchInput from "./searchInput";
import { ChipData, RemovableChipData } from "./types";
import DefaultChip from "./defaultChip";
import DefaultListItem from "./defaultListItem";
import "./inputChips.scss";

interface Props {
    renderItem?: (selected: boolean, value: ChipData, handleSelect: (val: ChipData) => void) => JSX.Element;
    onChange?: (item: ChipData[]) => void;
    renderChip?: (chip: RemovableChipData) => JSX.Element;
    chipsData?: ChipData[];
    inputPlaceholder?: string;
    emptyMessage?: string;
    fetchSearchSuggestions?: (value: string) => Promise<ChipData[]>;
    searchIcon?: JSX.Element;
    suggestionList?: ChipData[];
    className?: string;
    loadingSpinner?: JSX.Element;
    inputClassName?: string;
    noIcon?: boolean;
}

export function InputChips(props: Props) {
    const inputRef = useRef<HTMLInputElement | null>(null);
    const [chipsData, setChipsData] = useState(props.chipsData || []);

    useEffect(() => {
        setChipsData(props.chipsData || []);
    }, [props.chipsData]);

    const changeChips = (chips: ChipData[]) => {
        setChipsData(chips);
        if (props.onChange) {
            props.onChange(chips);
        }
    };

    const onKeyPress = (event: React.KeyboardEvent<HTMLInputElement>) => {
        const tmpTarget = event.target as HTMLInputElement;
        if (event.key === "Backspace" && tmpTarget.value === "") {
            const tmpChipsData = [...chipsData];
            tmpChipsData.pop();
            changeChips(tmpChipsData);
        }
    };

    const addItem = (item: ChipData) => {
        if (!chipsData.some((chip) => chip.id === item.id)) {
            const tmpChipsData = [...chipsData, item];
            changeChips(tmpChipsData);
        }
    };

    const removeChip = (chip: ChipData) => {
        const tmpChipsData = [...chipsData];
        const filteredData = tmpChipsData.filter((item) => item.id !== chip.id);
        changeChips(filteredData);
    };

    const renderChip = (chip: ChipData) => {
        const tmpChip: RemovableChipData = {
            ...chip,
            onRemove: removeChip,
        };
        if (props.renderChip) {
            return props.renderChip(tmpChip);
        }
    };

    const inputSetting = (input: HTMLInputElement) => {
        inputRef.current = input;
    };

    const onKeyDownItem = (event: React.KeyboardEvent<HTMLInputElement>) => {
        onKeyPress(event);
    };

    const onClickItem = () => {
        if (inputRef.current) {
            inputRef.current.focus();
        }
    };

    const renderListItem = (selected: boolean, value: ChipData, handleSelect: (val: ChipData) => void) => {
        if (props.renderItem) {
            props.renderItem(selected, value, handleSelect);
        }
        return (
            <DefaultListItem
                key={value.id}
                preSelected={chipsData.includes(value)}
                value={value}
                selected={selected}
                handleSelect={handleSelect}
            />
        );
    };
    return (
        <div onKeyDown={onKeyDownItem} onClick={onClickItem} className={`input_chips_wrapper ${props.className || ""}`}>
            {chipsData && chipsData.map((item) => renderChip(item))}
            <SearchInput
                loadingSpinner={props.loadingSpinner}
                fetchSearchSuggestions={props.fetchSearchSuggestions}
                suggestionList={props.suggestionList}
                minLength={1}
                inputClassName={props.inputClassName || "chips-input"}
                debounceTimeout={250}
                handleSelectElement={addItem}
                renderListItem={renderListItem}
                setInputRef={inputSetting}
                inputPlaceholder={props.inputPlaceholder || "Search"}
                emptyMessage={props.emptyMessage}
            />
        </div>
    );
}

InputChips.defaultProps = {
    chipsData: [],
    suggestionList: [],
    emptyMessage: "",
    inputPlaceholder: "Add tag",
    renderChip: (value: RemovableChipData) => <DefaultChip key={value.id} value={value} />,
};

export type { ChipData, RemovableChipData } from "./types";

export default InputChips;
