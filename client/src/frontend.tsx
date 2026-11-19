/**
 * This file is the entry point for the React app, it sets up the root
 * element and renders the App component to the DOM.
 *
 * It is included in `src/index.html`.
 */

import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter, Route, Routes } from "react-router";
import { MainPage } from "./Pages/MainPage.tsx";
import { RequestVacationPage } from "./Pages/RequestVacationPage.tsx";
import { VacationBalancePage } from "./Pages/VacationBalancePage.tsx";

const elem = document.getElementById("root")!;
const app = (
    <StrictMode>
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<MainPage />} />
                <Route path="/anmod-ferie" element={<RequestVacationPage />} />
                <Route path="/feriesaldo" element={<VacationBalancePage />} />
            </Routes>
        </BrowserRouter>
    </StrictMode>
);

// https://bun.com/docs/bundler/hot-reloading#import-meta-hot-data
(import.meta.hot.data.root ??= createRoot(elem)).render(app);