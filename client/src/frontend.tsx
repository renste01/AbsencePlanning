
/**
 * This file is the entry point for the React app, it sets up the root
 * element and renders the App component to the DOM.
 *
 * It is included in `src/index.html`.
 */

import { StrictMode, useState } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter, Route, Routes } from "react-router";
import { LogIn } from "./Pages/LogInPage.tsx";
import { MainPage } from "./Pages/MainPage.tsx";
import { RequestVacationPage } from "./Pages/RequestVacationPage.tsx";
import { VacationBalancePage } from "./Pages/VacationBalancePage.tsx";
import { login } from "./auth.ts";
import { ApproveRequestsPage } from "./Pages/ApproveRequestsPage.tsx";
import { CalendarPage } from "./Pages/CalendarPage.tsx";

function Root() {
    
    //change this back to usestate(false) later
    const [loggedIn, setLoggedIn] = useState(
        () => localStorage.getItem("absencePlanning_token") !== null
    );

    async function handleLogin(username: string, password: string) {
        await login(username, password);
        setLoggedIn(true);
    }

    if (!loggedIn) {
        return (
            <LogIn
                onLogin={handleLogin}
                onSwitchToRegister={() => {}}
            />
        );
    }

    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<MainPage />} />
                <Route path="/anmod-ferie" element={<RequestVacationPage />} />
                <Route path="/godkend-anmodninger" element={<ApproveRequestsPage />} />
                <Route path="/feriesaldo" element={<VacationBalancePage />} />
                <Route path="/kalender" element={<CalendarPage />} />
            </Routes>
        </BrowserRouter>
    );
}

const elem = document.getElementById("root")!;

const app = (
    <StrictMode>
        <Root />
    </StrictMode>
);

// https://bun.com/docs/bundler/hot-reloading#import-meta-hot-data
(import.meta.hot.data.root ??= createRoot(elem)).render(app);
