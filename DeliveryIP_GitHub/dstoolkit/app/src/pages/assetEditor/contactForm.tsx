import React, { ChangeEvent, useState } from "react";
import { useTranslation } from "react-i18next";
import { Button, InputOnChangeData, Textarea } from "@fluentui/react-components";
import { formStyles } from "../../components/form/formStyles";
import { ContactItem } from "../../types/contactItem";
import { useEffectOnce } from "../../utils/react/useEffectOnce";
import { useMsal } from "@azure/msal-react";

interface ContactFormProps {
    onSave: (data: ContactItem) => void;
}

export function ContactForm({ onSave }: ContactFormProps) {
    const { t } = useTranslation();
    const [data, setData] = useState<ContactItem>({} as ContactItem);
    const classes = formStyles();
    const { accounts } = useMsal();

    useEffectOnce(() => {
        if (accounts.length > 0) setData({ from: accounts[0].username, body: "" });
    });

    function onInputChanged(ev: ChangeEvent<HTMLElement>, input: InputOnChangeData): void {
        const property = (ev.target as HTMLInputElement).name;
        setData(
            (d) =>
                ({
                    ...d,
                    [property]: input.value === undefined || input.value.length === 0 ? "" : input.value,
                } as ContactItem)
        );
    }

    return (
        <>
            <div className="font-bold uppercase">{t("pages.editor.contact-title")}</div>
            <div className="mt-2 leading-loose">
                {t("pages.editor.contact-intro")
                    .split("\n")
                    .map((s, i) =>
                        s.startsWith("-") ? (
                            <li key={i} className="pl-4">
                                {s.substring(1)}
                            </li>
                        ) : (
                            <p key={i}>{s}</p>
                        )
                    )}
            </div>
            <div className="mt-6 font-bold uppercase">{t("pages.editor.contact-optional")}</div>
            <div className="mt-2">{t("pages.editor.contact-additional")}</div>

            <div className="mt-6 flex">
                <Textarea
                    className="w-full"
                    name="body"
                    onChange={onInputChanged}
                    value={data.body || ""}
                    maxLength={400}
                    placeholder={t("pages.editor.contact-placeholder")}
                    textarea={{ className: classes.textAreaStyles }}
                />
            </div>

            <div className="mt-4 flex">
                <Button appearance="primary" onClick={() => onSave(data)}>
                    {t("common.submit")}
                </Button>
            </div>
        </>
    );
}
