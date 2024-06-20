import React from "react";

interface CounterProps {
    value: string;
    className?: string;
}

export function Counter({ value, className }: CounterProps) {
    return (
        <div
            className={`${
                className || ""
            } flex h-6 w-6 shrink-0 cursor-default items-center justify-center rounded-full bg-neutral-100 text-[10px] font-normal text-neutral-700`}
        >
            {value}
        </div>
    );
}
