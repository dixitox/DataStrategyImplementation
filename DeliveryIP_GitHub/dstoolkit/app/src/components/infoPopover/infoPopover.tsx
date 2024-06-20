import React from "react";
import { Popover, PopoverTrigger, Button, PopoverSurface } from "@fluentui/react-components";
import { Info16Regular } from "@fluentui/react-icons";
import "./infoPopover.scss";

export function InfoPopover({ children, className }: { children: React.ReactNode; className?: string }) {
    return (
        <Popover withArrow>
            <PopoverTrigger disableButtonEnhancement>
                <span className={`inline-block align-middle ${className || ""}`}>
                    <Button size="small" appearance="transparent" icon={<Info16Regular />} />
                </span>
            </PopoverTrigger>
            <PopoverSurface className="_infopopover max-w-screen-sm whitespace-pre-line md:max-w-screen-md">
                {children}
            </PopoverSurface>
        </Popover>
    );
}
