import React, { ChangeEvent, useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { Input, InputOnChangeData, Label, Textarea } from "@fluentui/react-components";
import { formStyles } from "../../components/form/formStyles";
import { InfoPopover } from "../../components/infoPopover/infoPopover";

interface AssetTextInputProps {
    fieldName: string;
    value?: string;
    className?: string;
    multiline?: boolean;
    label?: string;
    placeholder?: string;
    extendedInfo?: React.ReactNode | string;
    onChange?: (ev: ChangeEvent<HTMLElement>, data: InputOnChangeData) => void;
}

export function AssetTextInput({
    fieldName,
    value,
    className,
    multiline = false,
    label,
    placeholder,
    extendedInfo,
    onChange,
}: AssetTextInputProps) {
    const { t } = useTranslation();
    const classes = formStyles();

    const [dashedName, setDashedName] = useState<string>();

    useEffect(() => {
        // Convert camelCase into dashed-case and ignore index values (e.g. [0])
        setDashedName(fieldName.replace(/[A-Z]/g, (m) => "-" + m.toLowerCase()).replace(/\[\d+\]/g, ""));
    }, [fieldName]);

    return dashedName ? (
        <>
            <Label className={classes.labelStyles} htmlFor={`input-${fieldName}`}>
                {label || t(`entities.asset.${dashedName}`)}
                {extendedInfo && <InfoPopover className="-mt-3 -ml-1">{extendedInfo}</InfoPopover>}
            </Label>
            {!multiline && (
                <Input
                    className={`grow ${className ? className : ""}`}
                    id={`input-${fieldName}`}
                    name={fieldName}
                    onChange={onChange}
                    value={value || ""}
                    type="text"
                    maxLength={200}
                    placeholder={placeholder || t(`entities.asset.${dashedName}`)}
                />
            )}
            {multiline && (
                <Textarea
                    className={`grow ${className ? className : ""}`}
                    id={`input-${fieldName}`}
                    name={fieldName}
                    onChange={onChange}
                    value={value || ""}
                    maxLength={2000}
                    placeholder={placeholder || t(`entities.asset.${dashedName}`)}
                    textarea={{ className: classes.textAreaStyles }}
                />
            )}
        </>
    ) : null;
}
