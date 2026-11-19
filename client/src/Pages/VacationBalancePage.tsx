import { useState } from "react";
import { BE_COLOURS as COLOURS } from "@/Colours.tsx";
import { PageShell } from "@/Components/PageShell.tsx";
import "../index.css";

type Account = "weeks1to5" | "week6";

type HolidayPeriod = {
    id: string;
    account: Account;
    earnStart: Date;
    earnEnd: Date;
    usableFrom: Date;
    usableTo: Date;
    earned: number;
    taken: number;
    planned: number;
    accrualPerMonth: number;
};

const ACCOUNTS: Account[] = ["weeks1to5", "week6"];

const ACCOUNT_LABELS: Record<Account, string> = {
    weeks1to5: "Uge 1-5 (Ferieloven)",
    week6: "6. ferieuge (KL-aftalen)",
};

const PERIODS: HolidayPeriod[] = [
    {
        id: "w15-2025",
        account: "weeks1to5",
        earnStart: new Date(2025, 8, 1),
        earnEnd: new Date(2026, 7, 31),
        usableFrom: new Date(2025, 8, 1),
        usableTo: new Date(2027, 11, 31),
        earned: 185,
        taken: 148.5,
        planned: 15,
        accrualPerMonth: 15.39,
    },
    {
        id: "w15-2026",
        account: "weeks1to5",
        earnStart: new Date(2026, 8, 1),
        earnEnd: new Date(2027, 7, 31),
        usableFrom: new Date(2026, 8, 1),
        usableTo: new Date(2028, 11, 31),
        earned: 30.78,
        taken: 0,
        planned: 0,
        accrualPerMonth: 15.39,
    },
    {
        id: "w6-2025",
        account: "week6",
        earnStart: new Date(2025, 0, 1),
        earnEnd: new Date(2025, 11, 31),
        usableFrom: new Date(2026, 4, 1),
        usableTo: new Date(2027, 3, 30),
        earned: 37,
        taken: 22.5,
        planned: 0,
        accrualPerMonth: 3.083,
    },
    {
        id: "w6-2026",
        account: "week6",
        earnStart: new Date(2026, 0, 1),
        earnEnd: new Date(2026, 11, 31),
        usableFrom: new Date(2027, 4, 1),
        usableTo: new Date(2028, 3, 30),
        earned: 33.91,
        taken: 0,
        planned: 0,
        accrualPerMonth: 3.083,
    },
];

const MAX_DEBT_HOURS = 37;
const AVERAGE_DAY_HOURS = 7.4;
const EXPIRY_WARNING_MONTHS = 6;

const startOfToday = () => {
    const now = new Date();
    return new Date(now.getFullYear(), now.getMonth(), now.getDate());
};

const toIso = (date: Date) => {
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return `${date.getFullYear()}-${month}-${day}`;
};

const parseIso = (value: string) => {
    const [year, month, day] = value.split("-").map(Number);
    return new Date(year!, month! - 1, day!);
};

const addMonths = (date: Date, amount: number) => {
    const next = new Date(date);
    next.setMonth(next.getMonth() + amount);
    return next;
};

const monthsBetween = (from: Date, to: Date) =>
    Math.max(0, (to.getFullYear() - from.getFullYear()) * 12 + to.getMonth() - from.getMonth());

const fmtDate = (date: Date) =>
    date.toLocaleDateString("da-DK", { day: "numeric", month: "short", year: "numeric" });

const fmtHours = (hours: number) =>
    `${hours.toLocaleString("da-DK", { minimumFractionDigits: 2, maximumFractionDigits: 2 })} t`;

const earnedOn = (period: HolidayPeriod, date: Date, today: Date) => {
    const limit = date < period.earnEnd ? date : period.earnEnd;
    return period.earned + period.accrualPerMonth * monthsBetween(today, limit);
};

const remainingOn = (period: HolidayPeriod, date: Date, today: Date) =>
    earnedOn(period, date, today) - period.taken - period.planned;

const isUsableOn = (period: HolidayPeriod, date: Date) =>
    date >= period.usableFrom && date <= period.usableTo;

const availableOn = (account: Account, date: Date, today: Date) =>
    PERIODS.filter((period) => period.account === account && isUsableOn(period, date)).reduce(
        (sum, period) => sum + remainingOn(period, date, today),
        0
    );

const expiredBefore = (account: Account, date: Date, today: Date) =>
    PERIODS.filter((period) => period.account === account && period.usableTo < date).reduce(
        (sum, period) => sum + Math.max(0, remainingOn(period, today, today)),
        0
    );

export function VacationBalancePage() {
    const today = startOfToday();
    const defaultTarget = new Date(today.getFullYear(), 11, 31);
    const [targetDate, setTargetDate] = useState(
        toIso(defaultTarget < today ? new Date(today.getFullYear() + 1, 11, 31) : defaultTarget)
    );

    const c = COLOURS;
    const field = { background: c.surface, borderColor: c.border, color: c.text };
    const card = { background: c.surface, borderColor: c.border };

    const target = targetDate ? parseIso(targetDate) : null;
    const targetInPast = target !== null && target < today;

    return (
        <PageShell
            title="Feriesaldo"
            subtitle="Overblik over dine feriekonti"
            intro="Se hvor mange ferietimer du har, hvornår de udløber, og hvad din saldo bliver på en fremtidig dato."
            activeNav="feriesaldo"
        >
            <main className="mp__grid">
                {ACCOUNTS.map((account) => {
                    const periods = PERIODS.filter((period) => period.account === account);
                    const available = availableOn(account, today, today);

                    return (
                        <section key={account} className="mp__card" style={card}>
                            <h2>{ACCOUNT_LABELS[account]}</h2>

                            <div className="mp__stat">
                                <span style={{ color: c.muted }}>Kan afholdes nu</span>
                                <strong style={{ color: available < 0 ? c.accent : c.primary }}>{fmtHours(available)}</strong>
                            </div>

                            {periods.map((period) => {
                                const remaining = remainingOn(period, today, today);
                                const usableNow = isUsableOn(period, today);
                                const expiresSoon =
                                    usableNow && remaining > 0 && period.usableTo <= addMonths(today, EXPIRY_WARNING_MONTHS);

                                return (
                                    <div key={period.id} className="mp__period" style={{ borderColor: c.border }}>
                                        <h3>Optjent {fmtDate(period.earnStart)} – {fmtDate(period.earnEnd)}</h3>
                                        <p className="mp__note" style={{ color: c.muted }}>
                                            {usableNow
                                                ? `Kan afholdes til og med ${fmtDate(period.usableTo)}`
                                                : `Kan først afholdes fra ${fmtDate(period.usableFrom)}`}
                                        </p>
                                        {expiresSoon && (
                                            <p className="mp__note" style={{ color: c.accent }}>
                                                Udløber snart. Timerne skal afholdes, overføres eller udbetales.
                                            </p>
                                        )}
                                        <div className="mp__totals">
                                            <div>
                                                <span style={{ color: c.muted }}>Optjent</span>
                                                <strong>{fmtHours(earnedOn(period, today, today))}</strong>
                                            </div>
                                            <div>
                                                <span style={{ color: c.muted }}>Afholdt</span>
                                                <strong>{fmtHours(period.taken)}</strong>
                                            </div>
                                            <div>
                                                <span style={{ color: c.muted }}>Planlagt (godkendt)</span>
                                                <strong>{fmtHours(period.planned)}</strong>
                                            </div>
                                            <div>
                                                <span style={{ color: c.muted }}>Tilbage</span>
                                                <strong style={{ color: remaining < 0 ? c.accent : c.text }}>{fmtHours(remaining)}</strong>
                                            </div>
                                        </div>
                                    </div>
                                );
                            })}
                        </section>
                    );
                })}

                <section className="mp__card mp__card--wide" style={card}>
                    <h2>Saldo på en fremtidig dato</h2>

                    <label className="mp__field">
                        Vælg dato
                        <input
                            type="date"
                            value={targetDate}
                            min={toIso(today)}
                            onChange={(event) => setTargetDate(event.target.value)}
                            style={field}
                        />
                    </label>

                    {target === null && (
                        <p className="mp__note" style={{ color: c.muted }}>Vælg en dato for at se din forventede saldo.</p>
                    )}

                    {targetInPast && (
                        <p className="mp__note" style={{ color: c.accent }}>Vælg en dato i fremtiden.</p>
                    )}

                    {target !== null && !targetInPast && (
                        <div className="mp__projection">
                            {ACCOUNTS.map((account) => {
                                const projected = availableOn(account, target, today);
                                const days = projected > 0 ? Math.floor(projected / AVERAGE_DAY_HOURS) : 0;
                                const expired = expiredBefore(account, target, today);

                                return (
                                    <div key={account} className="mp__result" style={{ borderColor: c.border }}>
                                        <span style={{ color: c.muted }}>{ACCOUNT_LABELS[account]}</span>
                                        <strong style={{ color: projected < 0 ? c.accent : c.primary }}>{fmtHours(projected)}</strong>
                                        <span style={{ color: c.muted }}>Svarer til ca. {days} hele arbejdsdage</span>
                                        {expired > 0 && (
                                            <p className="mp__note" style={{ color: c.accent }}>
                                                {fmtHours(expired)} udløber inden denne dato.
                                            </p>
                                        )}
                                    </div>
                                );
                            })}
                        </div>
                    )}

                    <p className="mp__note" style={{ color: c.muted }}>
                        Du må gå i minus med op til {fmtHours(MAX_DEBT_HOURS)}, men saldoen må ikke være negativ den 31. august.
                    </p>
                </section>
            </main>
        </PageShell>
    );
}

export default VacationBalancePage;