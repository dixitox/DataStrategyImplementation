import { AccountInfo } from "@azure/msal-browser";
import { ToolkitRoles } from "../../types/toolkitRoles";
import { RunFlags } from "../../types/runFlags";

export function isPlatformAdmin(accounts: AccountInfo[]): boolean {
    return checkRole(accounts, ToolkitRoles.PlatformAdmin);
}

export function isPlatformConsumer(accounts: AccountInfo[]): boolean {
    return (
        window.ENV?.RUN_FLAGS.includes(RunFlags.AllowAnonymousAccess)  ||
        (window.ENV?.RUN_FLAGS.includes(RunFlags.LoggedInAsConsumer) && isLoggedIn(accounts)) ||
        checkRole(accounts, ToolkitRoles.PlatformConsumer) ||
        checkRole(accounts, ToolkitRoles.PlatformProducer) ||
        checkRole(accounts, ToolkitRoles.PlatformAdmin)
    );
}

export function isPlatformProducer(accounts: AccountInfo[]): boolean {
    return checkRole(accounts, ToolkitRoles.PlatformProducer);
}

function checkRole(accounts: AccountInfo[], role: string): boolean {
    return (
        accounts.length > 0 &&
        (accounts[0].idTokenClaims?.roles || false) &&
        accounts[0].idTokenClaims.roles.includes(role)
    );
}

function isLoggedIn(accounts: AccountInfo[]): boolean {
    return accounts.length > 0 && accounts[0].idTokenClaims != undefined;
}
