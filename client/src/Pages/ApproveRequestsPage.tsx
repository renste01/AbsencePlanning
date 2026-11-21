import { useState, type ReactNode } from "react";
import { BE_COLOURS as COLOURS } from "@/Colours.tsx";
import { PageShell } from "@/Components/PageShell.tsx";
import "../index.css";

type Account = "weeks1to5" | "week6";
type RequestType = "ferie" | "annullering" | "overforsel";
type Status = "afventer" | "godkendt" | "afvist";
type View = "afventer" | "behandlede";

type VacationRequest = {
    id: string;
    employee: string;
    email: string;
    type: RequestType;
    account: Account;
    from: string | null;
    to: string | null;
    days: number;
    hours: number;
    balanceBefore: number;
    submitted: string;
    absentColleagues: number;
    warnings: string[];
    status: Status;
    reason?: string;
    decidedOn?: string;
};

const ACCOUNT_LABELS: Record<Account, string> = {
    weeks1to5: "Uge 1-5 (Ferieloven)",
    week6: "6. ferieuge (KL-aftalen)",
};

const TYPE_LABELS: Record<RequestType, string> = {
    ferie: "Ferieanmodning",
    annullering: "Annullering af ferie",
    overforsel: "Overførsel af ferie",
};

const STATUS_LABELS: Record<Status, string> = {
    afventer: "Afventer",
    godkendt: "Godkendt",
    afvist: "Afvist",
};

const INITIAL_REQUESTS: VacationRequest[] = [
    {
        id: "3f9c2a1e-6b1d-4c1e-9a52-1d7f0e6b8a01",
        employee: "Mette Hansen",
        email: "mette.hansen@businessesbjerg.com",
        type: "ferie",
        account: "weeks1to5",
        from: "2027-02-15",
        to: "2027-02-19",
        days: 5,
        hours: 37,
        balanceBefore: 118.5,
        submitted: "2026-11-12",
        absentColleagues: 1,
        warnings: [],
        status: "afventer",
    },
    {
        id: "8a4d7b20-2c93-4f8a-b3e1-5c9a47d2e602",
        employee: "Jonas Pedersen",
        email: "jonas.pedersen@businessesbjerg.com",
        type: "ferie",
        account: "weeks1to5",
        from: "2027-07-05",
        to: "2027-07-23",
        days: 15,
        hours: 111,
        balanceBefore: 140.25,
        submitted: "2026-11-18",
        absentColleagues: 3,
        warnings: [],
        status: "afventer",
    },
    {
        id: "b17e5c93-90a4-4d6b-8f0c-72e3a1d49b03",
        employee: "Sofie Nielsen",
        email: "sofie.nielsen@businessesbjerg.com",
        type: "ferie",
        account: "week6",
        from: "2027-06-14",
        to: "2027-06-16",
        days: 3,
        hours: 22.5,
        balanceBefore: 37,
        submitted: "2026-11-19",
        absentColleagues: 2,
        warnings: [],
        status: "afventer",
    },
    {
        id: "c52f8d14-e7b6-4a39-a1d0-9b4c63f05e04",
        employee: "Lars Christensen",
        email: "lars.christensen@businessesbjerg.com",
        type: "ferie",
        account: "weeks1to5",
        from: "2026-12-07",
        to: "2026-12-11",
        days: 5,
        hours: 37,
        balanceBefore: 20,
        submitted: "2026-11-19",
        absentColleagues: 4,
        warnings: ["Anmodet med mindre end 1 måneds varsel."],
        status: "afventer",
    },
    {
        id: "d96a3e57-14c8-4b2f-85e9-0a7d1c82f405",
        employee: "Anna Madsen",
        email: "anna.madsen@businessesbjerg.com",
        type: "annullering",
        account: "weeks1to5",
        from: "2027-03-01",
        to: "2027-03-05",
        days: 5,
        hours: 37,
        balanceBefore: 52,
        submitted: "2026-11-10",
        absentColleagues: 1,
        warnings: [],
        status: "afventer",
    },
    {
        id: "e03b9c68-5d27-4e81-96a4-3f8b2d1c7a06",
        employee: "Peter Olsen",
        email: "peter.olsen@businessesbjerg.com",
        type: "overforsel",
        account: "weeks1to5",
        from: null,
        to: null,
        days: 5,
        hours: 37,
        balanceBefore: 74,
        submitted: "2026-11-05",
        absentColleagues: 0,
        warnings: ["Virksomheden tillader i øjeblikket ikke overførsel af ferie."],
        status: "afventer",
    },
    {
        id: "f14c0d79-6e38-4f92-a7b5-4e9c3a2d8b07",
        employee: "Camilla Jensen",
        email: "camilla.jensen@businessesbjerg.com",
        type: "ferie",
        account: "weeks1to5",
        from: "2027-05-10",
        to: "2027-05-14",
        days: 5,
        hours: 37,
        balanceBefore: 96,
        submitted: "2026-11-13",
        absentColleagues: 1,
        warnings: [],
        status: "godkendt",
        decidedOn: "2026-11-17",
    },
    {
        id: "0a25e18a-7f49-4a03-b8c6-5fad4b3e9c08",
        employee: "Henrik Larsen",
        email: "henrik.larsen@businessesbjerg.com",
        type: "ferie",
        account: "weeks1to5",
        from: "2027-07-12",
        to: "2027-07-16",
        days: 5,
        hours: 37,
        balanceBefore: 88.5,
        submitted: "2026-11-11",
        absentColleagues: 5,
        warnings: [],
        status: "afvist",
        reason: "For mange kolleger er fraværende i perioden.",
        decidedOn: "2026-11-16",
    },
];

const MAX_DEBT_HOURS = 37;

const toIso = (date: Date) => {
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return `${date.getFullYear()}-${month}-${day}`;
};

const parseIso = (value: string) => {
    const [year, month, day] = value.split("-").map(Number);
    return new Date(year!, month! - 1, day!);
};

const fmtDate = (value: string) =>
    parseIso(value).toLocaleDateString("da-DK", { day: "numeric", month: "short", year: "numeric" });

const fmtHours = (hours: number) =>
    `${hours.toLocaleString("da-DK", { minimumFractionDigits: 2, maximumFractionDigits: 2 })} t`;

const balanceAfter = (request: VacationRequest) => {
    if (request.type === "ferie") return request.balanceBefore - request.hours;
    if (request.type === "annullering") return request.balanceBefore + request.hours;
    return request.balanceBefore;
};

const warningsFor = (request: VacationRequest) => {
    const messages = [...request.warnings];
    if (request.type === "ferie" && balanceAfter(request) < -MAX_DEBT_HOURS) {
        messages.push(`Overstiger det maksimale ferieunderskud på ${fmtHours(MAX_DEBT_HOURS)}.`);
    }
    return messages;
};

function Row({ label, children }: { label: string; children: ReactNode }) {
    return (
        <div>
            <span style={{ color: COLOURS.muted }}>{label}</span>
            <strong>{children}</strong>
        </div>
    );
}

export function ApproveRequestsPage() {
    const [requests, setRequests] = useState<VacationRequest[]>(INITIAL_REQUESTS);
    const [typeFilter, setTypeFilter] = useState<"alle" | RequestType>("alle");
    const [view, setView] = useState<View>("afventer");
    const [rejectingId, setRejectingId] = useState<string | null>(null);
    const [reason, setReason] = useState("");
    const [reasonMissing, setReasonMissing] = useState(false);

    const c = COLOURS;
    const field = { background: c.surface, borderColor: c.border, color: c.text };
    const card = { background: c.surface, borderColor: c.border };

    const pendingCount = requests.filter((request) => request.status === "afventer").length;
    const processedCount = requests.length - pendingCount;

    const visible = requests
        .filter((request) => (view === "afventer" ? request.status === "afventer" : request.status !== "afventer"))
        .filter((request) => typeFilter === "alle" || request.type === typeFilter)
        .sort((a, b) =>
            view === "afventer"
                ? a.submitted.localeCompare(b.submitted)
                : (b.decidedOn ?? "").localeCompare(a.decidedOn ?? "")
        );

    const decide = (id: string, status: Status, reasonText?: string) => {
        setRequests((previous) =>
            previous.map((request) =>
                request.id === id ? { ...request, status, reason: reasonText, decidedOn: toIso(new Date()) } : request
            )
        );
        setRejectingId(null);
        setReason("");
        setReasonMissing(false);
    };

    const startReject = (id: string) => {
        setRejectingId(id);
        setReason("");
        setReasonMissing(false);
    };

    const cancelReject = () => {
        setRejectingId(null);
        setReason("");
        setReasonMissing(false);
    };

    const confirmReject = (id: string) => {
        if (reason.trim() === "") {
            setReasonMissing(true);
            return;
        }
        decide(id, "afvist", reason.trim());
    };

    const badgeStyle = (status: Status) => {
        if (status === "godkendt") return { background: c.primary, color: c.surface };
        if (status === "afvist") return { background: c.accent, color: c.surface };
        return { background: c.accentSoft, color: c.accent };
    };

    const tabStyle = (active: boolean) =>
        active ? { background: c.primary, color: c.surface } : { background: c.accentSoft, color: c.accent };

    return (
        <PageShell
            title="Godkend anmodninger"
            subtitle="Behandl anmodninger fra medarbejdere"
            intro="Godkend eller afvis ferieanmodninger, annulleringer og overførsler. En afvisning kræver en begrundelse."
            activeNav={null}
        >
            <main className="mp__grid">
                <section className="mp__card mp__card--wide" style={card}>
                    <div className="mp__toolbar">
                        <div className="mp__tabs">
                            <button type="button" className="mp__btn" style={tabStyle(view === "afventer")}
                                    onClick={() => setView("afventer")}>
                                Afventer ({pendingCount})
                            </button>
                            <button type="button" className="mp__btn" style={tabStyle(view === "behandlede")}
                                    onClick={() => setView("behandlede")}>
                                Behandlede ({processedCount})
                            </button>
                        </div>

                        <label className="mp__field">
                            Type
                            <select value={typeFilter} onChange={(event) => setTypeFilter(event.target.value as "alle" | RequestType)}
                                    style={field}>
                                <option value="alle">Alle typer</option>
                                <option value="ferie">{TYPE_LABELS.ferie}</option>
                                <option value="annullering">{TYPE_LABELS.annullering}</option>
                                <option value="overforsel">{TYPE_LABELS.overforsel}</option>
                            </select>
                        </label>
                    </div>
                </section>

                {visible.length === 0 && (
                    <section className="mp__card mp__card--wide" style={card}>
                        <p className="mp__note" style={{ color: c.muted }}>Ingen anmodninger at vise.</p>
                    </section>
                )}

                {visible.map((request) => {
                    const after = balanceAfter(request);
                    const warnings = warningsFor(request);
                    const rejecting = rejectingId === request.id;

                    return (
                        <section key={request.id} className="mp__card" style={card}>
                            <div className="mp__req-head">
                                <div>
                                    <h2>{request.employee}</h2>
                                    <p className="mp__note" style={{ color: c.muted }}>{request.email}</p>
                                </div>
                                <span className="mp__badge" style={badgeStyle(request.status)}>
                                    {STATUS_LABELS[request.status]}
                                </span>
                            </div>

                            <div className="mp__totals">
                                <Row label="Type">{TYPE_LABELS[request.type]}</Row>
                                <Row label="Feriekonto">{ACCOUNT_LABELS[request.account]}</Row>
                                {request.from && request.to && (
                                    <Row label="Periode">{fmtDate(request.from)} – {fmtDate(request.to)}</Row>
                                )}
                                <Row label="Antal dage">{request.days}</Row>
                                <Row label="Timer">{fmtHours(request.hours)}</Row>
                                <Row label="Saldo nu">{fmtHours(request.balanceBefore)}</Row>
                                <Row label="Saldo efter godkendelse">
                                    <span style={{ color: after < 0 ? c.accent : c.text }}>{fmtHours(after)}</span>
                                </Row>
                                <Row label="Kolleger fraværende i perioden">{request.absentColleagues}</Row>
                                <Row label="Indsendt">{fmtDate(request.submitted)}</Row>
                            </div>

                            {warnings.length > 0 && (
                                <ul className="mp__errors" role="alert">
                                    {warnings.map((message) => (
                                        <li key={message} style={{ color: c.accent }}>{message}</li>
                                    ))}
                                </ul>
                            )}

                            {request.status === "afventer" && !rejecting && (
                                <div className="mp__actions">
                                    <button type="button" className="mp__btn" onClick={() => decide(request.id, "godkendt")}
                                            style={{ background: c.primary, color: c.surface }}>
                                        Godkend
                                    </button>
                                    <button type="button" className="mp__btn" onClick={() => startReject(request.id)}
                                            style={{ background: c.accentSoft, color: c.accent }}>
                                        Afvis
                                    </button>
                                </div>
                            )}

                            {request.status === "afventer" && rejecting && (
                                <>
                                    <label className="mp__field">
                                        Begrundelse for afvisning
                                        <textarea rows={3} value={reason} onChange={(event) => setReason(event.target.value)}
                                                  style={field} />
                                    </label>
                                    {reasonMissing && (
                                        <p className="mp__note" role="alert" style={{ color: c.accent }}>
                                            Skriv en begrundelse, før du afviser anmodningen.
                                        </p>
                                    )}
                                    <div className="mp__actions">
                                        <button type="button" className="mp__btn" onClick={() => confirmReject(request.id)}
                                                style={{ background: c.accent, color: c.surface }}>
                                            Bekræft afvisning
                                        </button>
                                        <button type="button" className="mp__btn" onClick={cancelReject}
                                                style={{ background: c.accentSoft, color: c.accent }}>
                                            Fortryd
                                        </button>
                                    </div>
                                </>
                            )}

                            {request.status !== "afventer" && (
                                <>
                                    <p className="mp__note" style={{ color: c.muted }}>
                                        {STATUS_LABELS[request.status]} den {request.decidedOn ? fmtDate(request.decidedOn) : ""}
                                    </p>
                                    {request.reason && (
                                        <p className="mp__note" style={{ color: c.text }}>Begrundelse: {request.reason}</p>
                                    )}
                                </>
                            )}
                        </section>
                    );
                })}
            </main>
        </PageShell>
    );
}

export default ApproveRequestsPage;