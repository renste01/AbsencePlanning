import { useEffect, useMemo, useState, type CSSProperties } from "react";
import { BE_COLOURS as COLOURS } from "@/Colours.tsx";
import "../index.css";

type CalendarEvent = {
    date: string; // "2026-12-24"
    title: string;
    type: "Vacation" | "PublicHoliday";
    employeeId: number | null;
};

const MONTHS = [
    "Januar", "Februar", "Marts", "April", "Maj", "Juni",
    "Juli", "August", "September", "Oktober", "November", "December",
];
const WEEKDAYS = ["Man", "Tir", "Ons", "Tor", "Fre", "Lør", "Søn"];
const MAX_EVENTS = 3;

const pad = (n: number) => String(n).padStart(2, "0");
const toIso = (y: number, m: number, d: number) => `${y}-${pad(m + 1)}-${pad(d)}`;

// ISO-ugenummer (dansk standard)
function isoWeek(y: number, m: number, d: number) {
    const date = new Date(Date.UTC(y, m, d));
    const day = date.getUTCDay() || 7;
    date.setUTCDate(date.getUTCDate() + 4 - day);
    const yearStart = new Date(Date.UTC(date.getUTCFullYear(), 0, 1));
    return Math.ceil(((date.getTime() - yearStart.getTime()) / 86400000 + 1) / 7);
}

export function CalendarPage() {
    const c = COLOURS;
    const today = new Date();
    const [year, setYear] = useState(today.getFullYear());
    const [month, setMonth] = useState(today.getMonth()); // 0-11
    const [events, setEvents] = useState<CalendarEvent[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    // Hent events for den valgte måned
    useEffect(() => {
        let cancelled = false;
        setLoading(true);
        setError(null);

        const lastDay = new Date(year, month + 1, 0).getDate();
        const url = `/api/calendar?fromDate=${toIso(year, month, 1)}&toDate=${toIso(year, month, lastDay)}`;

        fetch(url)
            .then((res) => {
                if (!res.ok) throw new Error(String(res.status));
                return res.json() as Promise<CalendarEvent[]>;
            })
            .then((data) => { if (!cancelled) setEvents(data); })
            .catch(() => { if (!cancelled) setError("Kunne ikke hente kalenderen."); })
            .finally(() => { if (!cancelled) setLoading(false); });

        return () => { cancelled = true; };
    }, [year, month]);

    // Gruppér events pr. dato, helligdage først
    const byDate = useMemo(() => {
        const map = new Map<string, CalendarEvent[]>();
        for (const e of events) {
            const list = map.get(e.date) ?? [];
            list.push(e);
            map.set(e.date, list);
        }
        for (const list of map.values()) {
            list.sort((a, b) => (a.type === b.type ? 0 : a.type === "PublicHoliday" ? -1 : 1));
        }
        return map;
    }, [events]);

    // Opbyg ugerne: mandag først, tomme felter før/efter måneden
    const weeks = useMemo(() => {
        const offset = (new Date(year, month, 1).getDay() + 6) % 7;
        const daysInMonth = new Date(year, month + 1, 0).getDate();
        const cells: (number | null)[] = [
            ...Array<null>(offset).fill(null),
            ...Array.from({ length: daysInMonth }, (_, i) => i + 1),
        ];
        while (cells.length % 7 !== 0) cells.push(null);

        const rows: { days: (number | null)[]; week: number }[] = [];
        for (let i = 0; i < cells.length; i += 7) {
            const days = cells.slice(i, i + 7);
            const anyDay = days.find((d) => d !== null) as number;
            rows.push({ days, week: isoWeek(year, month, anyDay) });
        }
        return rows;
    }, [year, month]);

    const goPrev = () => {
        if (month === 0) { setMonth(11); setYear(year - 1); } else setMonth(month - 1);
    };
    const goNext = () => {
        if (month === 11) { setMonth(0); setYear(year + 1); } else setMonth(month + 1);
    };
    const goToday = () => { setYear(today.getFullYear()); setMonth(today.getMonth()); };

    const isToday = (d: number) =>
        d === today.getDate() && month === today.getMonth() && year === today.getFullYear();

    const chip = { background: c.bg, borderColor: c.border, color: c.text };

    return (
        <div className="mp" style={{ background: c.bg, color: c.text }}>
            <div className="mp__content">
                <div className="mp__head" style={{ background: c.surface, borderColor: c.border }}>
                    <div className="mp__head-in">
                        <div className="mp__brand">
                            <span style={{ color: c.primary }}>BUS<b style={{ color: c.accent }}>IN</b>ESS</span>
                            <span style={{ color: c.primary }}>ESBJERG</span>
                        </div>
                        <div>
                            <h1>Fælles kalender</h1>
                            <p style={{ color: c.muted }}>Se hvem der har ferie</p>
                        </div>
                        <a className="mp__chip" href="/" style={{ ...chip, marginLeft: "auto", color: c.text }}>
                            Tilbage til overblik
                        </a>
                    </div>
                </div>

                <main className="cal" style={{ "--cal-border": c.border } as CSSProperties}>
                    <div className="cal__top">
                        <h2 className="cal__month" style={{ color: c.primary }}>{MONTHS[month]}</h2>
                        <div className="cal__nav">
                            <button type="button" className="mp__chip" style={chip} onClick={goPrev} aria-label="Forrige måned">‹</button>
                            <button type="button" className="mp__chip" style={chip} onClick={goToday}>I dag</button>
                            <button type="button" className="mp__chip" style={chip} onClick={goNext} aria-label="Næste måned">›</button>
                        </div>
                        <span className="cal__year" style={{ color: c.primary }}>{year}</span>
                    </div>

                    {error && <p style={{ color: c.accent }}>{error}</p>}

                    <div className="cal__grid" style={{ background: c.surface, opacity: loading ? 0.6 : 1 }}>
                        {WEEKDAYS.map((name) => (
                            <div key={name} className="cal__dayname">{name}</div>
                        ))}

                        {weeks.map((row) =>
                            row.days.map((day, i) => {
                                const iso = day ? toIso(year, month, day) : "";
                                const dayEvents = day ? byDate.get(iso) ?? [] : [];
                                const shown = dayEvents.slice(0, MAX_EVENTS);
                                const more = dayEvents.length - shown.length;
                                const hasHoliday = dayEvents.some((e) => e.type === "PublicHoliday");

                                return (
                                    <div
                                        key={`${row.week}-${i}`}
                                        className="cal__cell"
                                        style={isToday(day ?? -1) ? { boxShadow: `inset 0 0 0 2px ${c.accent}` } : undefined}
                                    >
                                        {day && (
                                            <span className="cal__num" style={{ color: hasHoliday ? c.accent : c.text }}>
                                                {day}
                                            </span>
                                        )}
                                        {i === 6 && (
                                            <span className="cal__week" style={{ color: c.muted }}>{row.week}</span>
                                        )}

                                        {shown.map((e, idx) => (
                                            <span
                                                key={idx}
                                                className="cal__ev"
                                                title={e.title}
                                                style={
                                                    e.type === "PublicHoliday"
                                                        ? { background: c.primary, color: c.surface }
                                                        : { background: c.accentSoft, color: c.accent }
                                                }
                                            >
                                                {e.title}
                                            </span>
                                        ))}
                                        {more > 0 && (
                                            <span
                                                className="cal__more"
                                                style={{ color: c.muted }}
                                                title={dayEvents.slice(MAX_EVENTS).map((e) => e.title).join("\n")}
                                            >
                                                +{more} flere
                                            </span>
                                        )}
                                    </div>
                                );
                            })
                        )}
                    </div>
                </main>
            </div>
        </div>
    );
}

export default CalendarPage;