import { useMemo, useState, type FormEvent } from "react";
import { BE_COLOURS as COLOURS } from "@/Colours.tsx";
import { PageShell } from "@/Components/PageShell.tsx";
import "../index.css";

type Account = "weeks1to5" | "week6";

type DayEntry = {
    date: Date;
    hours: number;
    skipReason: string | null;
};

const WORK_SCHEDULE: Record<number, number> = {
    0: 0,
    1: 7.5,
    2: 7.5,
    3: 7.5,
    4: 7.5,
    5: 7,
    6: 0,
};

const BALANCES: Record<Account, number> = {
    weeks1to5: 120,
    week6: 37,
};

const ACCOUNT_LABELS: Record<Account, string> = {
    weeks1to5: "Uge 1-5 (Ferieloven)",
    week6: "6. ferieuge (KL-aftalen)",
};

const MAX_DEBT_HOURS = 37;
const MAIN_HOLIDAY_DAYS = 15;

const toIso = (date: Date) => {
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return `${date.getFullYear()}-${month}-${day}`;
};

const parseIso = (value: string) => {
    const [year, month, day] = value.split("-").map(Number);
    return new Date(year!, month! - 1, day!);
};

const addDays = (date: Date, amount: number) => {
    const next = new Date(date);
    next.setDate(next.getDate() + amount);
    return next;
};

const addMonths = (date: Date, amount: number) => {
    const next = new Date(date);
    next.setMonth(next.getMonth() + amount);
    return next;
};

const easterSunday = (year: number) => {
    const a = year % 19;
    const b = Math.floor(year / 100);
    const c = year % 100;
    const d = Math.floor(b / 4);
    const e = b % 4;
    const f = Math.floor((b + 8) / 25);
    const g = Math.floor((b - f + 1) / 3);
    const h = (19 * a + b - d - g + 15) % 30;
    const i = Math.floor(c / 4);
    const k = c % 4;
    const l = (32 + 2 * e + 2 * i - h - k) % 7;
    const m = Math.floor((a + 11 * h + 22 * l) / 451);
    const month = Math.floor((h + l - 7 * m + 114) / 31);
    const day = ((h + l - 7 * m + 114) % 31) + 1;
    return new Date(year, month - 1, day);
};

const holidayName = (date: Date) => {
    const year = date.getFullYear();
    const easter = easterSunday(year);
    const named: Record<string, string> = {
        [`${year}-01-01`]: "Nytårsdag",
        [toIso(addDays(easter, -3))]: "Skærtorsdag",
        [toIso(addDays(easter, -2))]: "Langfredag",
        [toIso(addDays(easter, 1))]: "2. påskedag",
        [toIso(addDays(easter, 39))]: "Kristi himmelfartsdag",
        [toIso(addDays(easter, 50))]: "2. pinsedag",
        [`${year}-12-24`]: "Lukkedag",
        [`${year}-12-25`]: "Juledag",
        [`${year}-12-26`]: "2. juledag",
        [`${year}-12-31`]: "Lukkedag",
    };
    return named[toIso(date)] ?? null;
};

const buildDays = (start: Date, end: Date) => {
    const days: DayEntry[] = [];
    for (let cursor = start; cursor <= end; cursor = addDays(cursor, 1)) {
        const scheduled = WORK_SCHEDULE[cursor.getDay()] ?? 0;
        const holiday = holidayName(cursor);
        if (scheduled === 0) {
            days.push({ date: cursor, hours: 0, skipReason: "Weekend" });
        } else if (holiday) {
            days.push({ date: cursor, hours: 0, skipReason: holiday });
        } else {
            days.push({ date: cursor, hours: scheduled, skipReason: null });
        }
    }
    return days;
};

const fmtDate = (date: Date) =>
    date.toLocaleDateString("da-DK", { weekday: "short", day: "numeric", month: "short", year: "numeric" });

const fmtHours = (hours: number) =>
    `${hours.toLocaleString("da-DK", { minimumFractionDigits: 2, maximumFractionDigits: 2 })} t`;

const inMainWindow = (date: Date) => date.getMonth() >= 4 && date.getMonth() <= 8;

export function RequestVacationPage() {
    const [account, setAccount] = useState<Account>("weeks1to5");
    const [startDate, setStartDate] = useState("");
    const [endDate, setEndDate] = useState("");
    const [submitted, setSubmitted] = useState(false);

    const c = COLOURS;
    const field = { background: c.surface, borderColor: c.border, color: c.text };

    const days = useMemo(() => {
        if (!startDate || !endDate) return [];
        const start = parseIso(startDate);
        const end = parseIso(endDate);
        if (end < start) return [];
        return buildDays(start, end);
    }, [startDate, endDate]);

    const countedDays = days.filter((day) => day.skipReason === null);
    const totalHours = countedDays.reduce((sum, day) => sum + day.hours, 0);
    const balance = BALANCES[account];
    const balanceAfter = balance - totalHours;

    const errors = useMemo(() => {
        const messages: string[] = [];
        if (!startDate || !endDate) return messages;
        const start = parseIso(startDate);
        const end = parseIso(endDate);
        if (end < start) {
            messages.push("Slutdatoen kan ikke ligge før startdatoen.");
            return messages;
        }
        if (countedDays.length === 0) {
            messages.push("Perioden indeholder ingen arbejdsdage.");
            return messages;
        }
        const isMainHoliday =
            account === "weeks1to5" &&
            countedDays.length >= MAIN_HOLIDAY_DAYS &&
            inMainWindow(start) &&
            inMainWindow(end);
        const noticeMonths = isMainHoliday ? 3 : 1;
        if (account === "weeks1to5" && start < addMonths(new Date(), noticeMonths)) {
            messages.push(
                `${isMainHoliday ? "Hovedferie" : "Ferie"} skal anmodes mindst ${noticeMonths} ${noticeMonths === 1 ? "måned" : "måneder"} i forvejen.`
            );
        }
        if (balanceAfter < -MAX_DEBT_HOURS) {
            messages.push(`Anmodningen overstiger det maksimale ferieunderskud på ${fmtHours(MAX_DEBT_HOURS)}.`);
        }
        return messages;
    }, [startDate, endDate, account, countedDays.length, balanceAfter]);

    const canSubmit = startDate !== "" && endDate !== "" && countedDays.length > 0 && errors.length === 0;

    const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        if (canSubmit) setSubmitted(true);
    };

    const handleReset = () => {
        setStartDate("");
        setEndDate("");
        setSubmitted(false);
    };

    return (
        <PageShell
            title="Anmod om ferie"
            subtitle="Send en ferieanmodning til godkendelse"
            intro="Vælg feriekonto og periode. Der anmodes om hele dage, og saldoen trækkes i timer."
        >
            <form className="mp__grid" noValidate onSubmit={handleSubmit}>
                <section className="mp__card" style={{ background: c.surface, borderColor: c.border }}>
                    <h2>Periode</h2>

                    <label className="mp__field">
                        Feriekonto
                        <select
                            value={account}
                            onChange={(event) => {
                                setAccount(event.target.value as Account);
                                setSubmitted(false);
                            }}
                            style={field}
                        >
                            <option value="weeks1to5">{ACCOUNT_LABELS.weeks1to5}</option>
                            <option value="week6">{ACCOUNT_LABELS.week6}</option>
                        </select>
                    </label>

                    <div className="mp__balance" style={{ color: c.muted }}>
                        <span>Nuværende saldo</span>
                        <strong style={{ color: c.text }}>{fmtHours(balance)}</strong>
                    </div>

                    <div className="mp__row">
                        <label className="mp__field">
                            Fra
                            <input
                                type="date"
                                value={startDate}
                                onChange={(event) => {
                                    setStartDate(event.target.value);
                                    setSubmitted(false);
                                }}
                                style={field}
                            />
                        </label>

                        <label className="mp__field">
                            Til
                            <input
                                type="date"
                                value={endDate}
                                min={startDate}
                                onChange={(event) => {
                                    setEndDate(event.target.value);
                                    setSubmitted(false);
                                }}
                                style={field}
                            />
                        </label>
                    </div>
                </section>

                <section className="mp__card" style={{ background: c.surface, borderColor: c.border }}>
                    <h2>Oversigt</h2>

                    {days.length === 0 ? (
                        <p className="mp__empty" style={{ color: c.muted }}>Vælg en periode for at se oversigten.</p>
                    ) : (
                        <>
                            <ul className="mp__list mp__list--scroll">
                                {days.map((day) => (
                                    <li key={toIso(day.date)} style={{ borderColor: c.border, color: day.skipReason ? c.muted : c.text }}>
                                        <span>{fmtDate(day.date)}</span>
                                        <span>{day.skipReason ?? fmtHours(day.hours)}</span>
                                    </li>
                                ))}
                            </ul>

                            <div className="mp__totals">
                                <div>
                                    <span style={{ color: c.muted }}>Antal dage</span>
                                    <strong>{countedDays.length}</strong>
                                </div>
                                <div>
                                    <span style={{ color: c.muted }}>Timer der trækkes</span>
                                    <strong>{fmtHours(totalHours)}</strong>
                                </div>
                                <div>
                                    <span style={{ color: c.muted }}>Saldo efter godkendelse</span>
                                    <strong style={{ color: balanceAfter < 0 ? c.accent : c.text }}>{fmtHours(balanceAfter)}</strong>
                                </div>
                            </div>
                        </>
                    )}

                    {errors.length > 0 && (
                        <ul className="mp__errors" role="alert">
                            {errors.map((message) => (
                                <li key={message} style={{ color: c.accent }}>{message}</li>
                            ))}
                        </ul>
                    )}

                    {submitted ? (
                        <div className="mp__confirm">
                            <p style={{ color: c.text }}>Din anmodning er sendt til godkendelse.</p>
                            <button type="button" className="mp__btn" onClick={handleReset}
                                    style={{ background: c.accentSoft, color: c.accent }}>
                                Ny anmodning
                            </button>
                        </div>
                    ) : (
                        <button type="submit" className="mp__btn" disabled={!canSubmit}
                                style={{ background: c.primary, color: c.surface }}>
                            Send anmodning
                        </button>
                    )}
                </section>
            </form>
        </PageShell>
    );
}

export default RequestVacationPage;