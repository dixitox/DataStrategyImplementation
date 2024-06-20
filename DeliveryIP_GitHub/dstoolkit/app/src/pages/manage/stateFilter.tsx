import React, { useEffect } from "react";
import { Checkbox } from "@fluentui/react-checkbox";
import { useTranslation } from "react-i18next";
import { Link, makeStyles } from "@fluentui/react-components";
import { useEffectOnce } from "../../utils/react/useEffectOnce";

const useStyles = makeStyles({
    clearAll: { fontSize: "12px", fontWeight: "600" },
});

interface StateFilterProps {
    filterTitle?: string;
    filterOptions?: string;
    className?: string;
    onFilterChanged: (filter: boolean | undefined) => void;
}

export function StateFilter({ filterTitle, filterOptions, className, onFilterChanged }: StateFilterProps) {
    const { t } = useTranslation();

    const [allFilter, setAllFilter] = React.useState<string[]>([]);
    const [checkedFilter, setCheckedFilter] = React.useState<string[]>([]);

    const classes = useStyles();

    // Custom hook that can be used instead of useEffect() with zero dependencies.
    // Avoids a double execution of the effect when in React 18 DEV mode with <React.StrictMode>
    useEffectOnce(() => {
        setAllFilter(["true", "false"]);
    });

    useEffect(() => {
        // If all facets are selected send empty filter list
        let result: boolean | undefined = undefined;
        if (checkedFilter.length === 1) result = checkedFilter[0] === "false";
        onFilterChanged(result);
    }, [checkedFilter]);

    function clearAll() {
        setCheckedFilter([]);
    }

    return (
        <div className={`${className || ""} flex flex-col md:sticky md:top-5 md:self-start`}>
            <div className="flex items-center justify-between border-b border-b-neutral-300 pb-2">
                <span className="text-lg">{t("components.filter.title")}</span>
                <Link className={classes.clearAll} onClick={clearAll}>
                    {t("components.filter.clear-all")}
                </Link>
            </div>
            
            {filterTitle != undefined && 
            <span className="mt-4 mb-2 text-base font-semibold">
                    {t(filterTitle)}
            </span>
            }

            {allFilter.map((item, idx) => {
                return (
                    <Checkbox
                        key={idx}
                        className="-ml-2"
                        checked={checkedFilter.includes(item)}
                        onChange={(_ev, data) =>
                            data.checked
                                ? setCheckedFilter((o) => [...o, item])
                                : setCheckedFilter((o) => o.filter((o) => o !== item))
                        }
                        label={t(`${filterOptions + item}`)}
                    />
                );
            })}
            <span className="my-5 border-b border-b-neutral-300" />
        </div>
    );
}
