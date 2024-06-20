import React, { useEffect } from "react";
import { Routes, Route, Outlet } from "react-router-dom";
import { useMsal } from "@azure/msal-react";
import { InteractionStatus, RedirectRequest } from "@azure/msal-browser";
import { Auth } from "./utils/auth/auth";
import { Home } from "./pages/home/home";
import { Asset } from "./pages/asset/asset";
import { AssetEditor } from "./pages/assetEditor/assetEditor";
import { Manage } from "./pages/manage/manage";
import { DeploymentHistory } from "./pages/deploymentHistory/deploymentHistory";
import { isPlatformAdmin, isPlatformConsumer, isPlatformProducer } from "./utils/auth/roles";
import { AnalyticsReport } from "./pages/analyticsReport/analyticsReport";
import { BlogList } from "./pages/blogList/blogList";
import { BlogEditor } from "./pages/blogEditor/blogEditor";
import { BlogViewer } from "./pages/blogViewer/blogViewer";

function App() {
    const { accounts } = useMsal();

    return (
        <Routes>
            <Route
                path="/"
                element={
                    <ProtectedRoute isAllowed={isPlatformConsumer(accounts)}>
                        <Home />
                    </ProtectedRoute>
                }
            />
            <Route
                path="/search"
                element={
                    <ProtectedRoute isAllowed={isPlatformConsumer(accounts)}>
                        <Home isSearchResultsPage={true} />
                    </ProtectedRoute>
                }
            />
            <Route
                path="/assets/:assetId"
                element={
                    <ProtectedRoute isAllowed={isPlatformConsumer(accounts)}>
                        <Asset />
                    </ProtectedRoute>
                }
            />
            <Route
                path="/editor"
                element={
                    <ProtectedRoute isAllowed={isPlatformProducer(accounts) || isPlatformAdmin(accounts)}>
                        <AssetEditor key="new" />
                    </ProtectedRoute>
                }
            />
            <Route
                path="/editor/:assetId"
                element={
                    <ProtectedRoute isAllowed={isPlatformProducer(accounts) || isPlatformAdmin(accounts)}>
                        <AssetEditor key="edit" />
                    </ProtectedRoute>
                }
            />
            <Route
                path="/manage"
                element={
                    <ProtectedRoute isAllowed={isPlatformProducer(accounts) || isPlatformAdmin(accounts)}>
                        <Manage />
                    </ProtectedRoute>
                }
            />
            <Route
                path="deployment-history/:assetId"
                element={
                    <ProtectedRoute isAllowed={isPlatformProducer(accounts)|| isPlatformAdmin(accounts)}>
                        <DeploymentHistory />
                    </ProtectedRoute>
                }
            />
            <Route
                path="/analytics-report"
                element={
                    <ProtectedRoute isAllowed={isPlatformAdmin(accounts)}>
                        <AnalyticsReport />
                    </ProtectedRoute>
                }
            />
            <Route
                path="/blog"
                element={
                    <ProtectedRoute isAllowed={isPlatformConsumer(accounts)}>
                        <BlogList key="blog" />
                    </ProtectedRoute>
                }
            />
            <Route
                path="/blog/:blogId"
                element={
                    <ProtectedRoute isAllowed={isPlatformConsumer(accounts)}>
                        <BlogViewer key="blog-view" />
                    </ProtectedRoute>
                }
            />
            <Route
                path="/blog-editor"
                element={
                    <ProtectedRoute isAllowed={isPlatformAdmin(accounts)}>
                        <BlogEditor key="blog-new" />
                    </ProtectedRoute>
                }
            />
            <Route
                path="/blog-editor/:blogId"
                element={
                    <ProtectedRoute isAllowed={isPlatformAdmin(accounts)}>
                        <BlogEditor key="blog-edit" />
                    </ProtectedRoute>
                }
            />
            <Route path="*" element={<NotFound />} />
        </Routes>
    );
}

function ProtectedRoute({ isAllowed, children }: { isAllowed?: boolean; children: JSX.Element }): JSX.Element | null {
    const { instance, inProgress } = useMsal();

    if (isAllowed === undefined) isAllowed = instance.getActiveAccount() !== null;

    useEffect(() => {
        // Force user login if he isn't and has no access.
        if (!isAllowed && inProgress === InteractionStatus.None && instance.getActiveAccount() == null) {
            instance.loginRedirect(Auth.getAuthenticationRequest() as RedirectRequest);
        }
    }, [inProgress]);

    if (inProgress && inProgress === InteractionStatus.None) {
        if (isAllowed) return children ? children : <Outlet />;
        else return <Unauthorized />;
    } else {
        return null;
    }
}

function NotFound() {
    return (
        <main className="p-8 md:px-24">
            <h1>Not Found</h1>
        </main>
    );
}

function Unauthorized() {
    const { instance } = useMsal();

    function signOut() {
        instance.logoutRedirect();
    }
    return (
        <main className="p-8 md:px-24">
            <h1>Unauthorized</h1>
            <button onClick={signOut}>Logout</button>
        </main>
    );
}
export default App;
