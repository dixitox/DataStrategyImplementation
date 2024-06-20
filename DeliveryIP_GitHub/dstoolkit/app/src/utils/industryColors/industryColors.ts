const IndustriesColors: Record<string, string> = {
    Automotives: "#847545",
    Manufacturing: "#744DA9",
    Retail: "#68768A",
    Energy: "#D13438",
    Agriculture: "#498205",
    Education: "#486860",
    Sustainability: "#B146C2",
    Media: "#2D7D9A",
    "Financial Services & Insurance": "#4A5459",
    Other: "#050505",
};

export function getIndustryColor(industry: string): string {
    return IndustriesColors[industry] || IndustriesColors["Other"];
}
export function getIndustriesColors(industries: string[]): string {
    if (industries.length === 0) return IndustriesColors["Other"];
    if (industries.length === 1) return IndustriesColors[industries[0]] || IndustriesColors["Other"];
    else {
        const sequence = industries
            .map((industry) => IndustriesColors[industry] || IndustriesColors["Other"])
            .join(",");
        return `linear-gradient(to right, ${sequence})`;
    }
}
