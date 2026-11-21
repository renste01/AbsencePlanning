import { useRef, useState, type ReactNode } from "react";
import { BE_COLOURS as COLOURS } from "@/Colours.tsx";
import "../index.css";
import { Link } from "react-router";

type VacationRequest = {
    id: number;
    from: string;
    to: string;
    days: number;
    status: "Afventer" | "Godkendt" | "Afvist";
};

const fmt = (iso: string) =>
    new Date(iso).toLocaleDateString("da-DK", { day: "numeric", month: "short", year: "numeric" });

function Icon({ size = 20, stroke = 1.6, children }: { size?: number; stroke?: number; children: ReactNode }) {
    return (
        <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor"
             strokeWidth={stroke} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            {children}
        </svg>
    );
}
const UserIcon = (p: { size?: number }) => <Icon {...p}><circle cx="12" cy="12" r="9" /><circle cx="12" cy="10" r="3" /><path d="M6.2 18.5c1.3-2 3.3-3 5.8-3s4.5 1 5.8 3" /></Icon>;
const CalIcon = (p: { size?: number; stroke?: number }) => <Icon {...p}><rect x="3" y="4" width="18" height="18" rx="2" /><path d="M16 2v4M8 2v4M3 10h18" /></Icon>;
const CalPlusIcon = (p: { size?: number; stroke?: number }) => <Icon {...p}><rect x="3" y="4" width="18" height="18" rx="2" /><path d="M16 2v4M8 2v4M3 10h18M12 14v4M10 16h4" /></Icon>;
const ListIcon = (p: { size?: number; stroke?: number }) => <Icon {...p}><path d="M8 6h13M8 12h13M8 18h13M3 6h.01M3 12h.01M3 18h.01" /></Icon>;
const ClockIcon = (p: { size?: number; stroke?: number }) => <Icon {...p}><circle cx="12" cy="12" r="9" /><path d="M12 7v5l3 2" /></Icon>;
const HelpIcon = (p: { size?: number }) => <Icon {...p}><circle cx="12" cy="12" r="9" /><path d="M9.5 9.5a2.5 2.5 0 1 1 3.5 2.3c-.6.3-1 .8-1 1.5M12 17h.01" /></Icon>;
const PlaneIcon = (p: { size?: number; stroke?: number }) => <Icon {...p}><path d="M17.8 19.2 16 11l3.5-3.5C21 6 21.5 4 21 3c-1-.5-3 0-4.5 1.5L13 8 4.8 6.2c-.5-.1-.9.1-1.1.5l-.3.5c-.2.5-.1 1 .3 1.3L9 12l-2 3H4l-1 1 3 2 2 3 1-1v-3l3-2 3.5 5.3c.3.4.8.5 1.3.3l.5-.2c.4-.3.6-.7.5-1.2z" /></Icon>;

export function MainPage() {
    // TODO: hent fra backend
    const company = "Din virksomhed";

    const listDialog = useRef<HTMLDialogElement>(null);
    const [collapsed, setCollapsed] = useState(false);
    // TODO: hent brugerens anmodninger fra backend
    const [requests] = useState<VacationRequest[]>([]);

    const c = COLOURS;
    const tile = { background: c.surface, borderColor: c.border, color: c.text };
    const chip = { background: c.bg, borderColor: c.border, color: c.text };

    return (
        <div className="mp" style={{ background: c.bg, color: c.text }}>
            <aside className={`mp__side${collapsed ? " mp__side--collapsed" : ""}`} style={{ background: c.surface, borderColor: c.border }}>
                <div>
                    <div className="mp__side-top">
                        <div className="mp__logo" style={{ background: c.accentSoft, color: c.accent }}>BE</div>
                        <button type="button" className="mp__collapse" style={{ background: c.bg, color: c.text }}
                                onClick={() => setCollapsed(!collapsed)} aria-label={collapsed ? "Vis menu" : "Skjul menu"}>
                            <Icon size={18}><path d="M15 6l-6 6 6 6" /></Icon>
                        </button>
                    </div>
                    <a className="mp__sidelink" href="/profil" style={{ color: c.text }}><UserIcon size={18} /><span>Min konto</span></a>
                    <a className="mp__sidelink" href="/ferie" style={{ background: c.accentSoft, color: c.accent }}><CalIcon size={18} /><span>Ferie</span></a>
                    <a className="mp__sidelink" href="/feriesaldo" style={{ color: c.text }}><ClockIcon size={18} /><span>Feriesaldo</span></a>
                </div>
                <a className="mp__sidelink" href="/hjaelp" style={{ color: c.text }}><HelpIcon size={18} /><span>Hjælp</span></a>
            </aside>

            <div className="mp__content">
                <header className="mp__nav" style={{ background: c.surface, borderColor: c.border }}>
                    <nav className="mp__links" aria-label="Hovedmenu">
                        <a href="/" aria-current="page" style={{ background: c.accentSoft, color: c.accent }}>Overblik</a>
                        <a href="/profil" style={{ color: c.text }}>Profil</a>
                        <details className="mp__dd">
                            <summary style={{ color: c.text }}>Registreringer <span aria-hidden="true">▾</span></summary>
                            <div style={{ background: c.surface, borderColor: c.border }}>
                                <a href="/ferie" style={{ color: c.text }}>Ferie</a>
                                <a href="/fravaer" style={{ color: c.text }}>Fravær</a>
                            </div>
                        </details>
                    </nav>
                    <div className="mp__right">
                        <button type="button" className="mp__chip" style={chip}>Notifikationer</button>
                        <span className="mp__chip" style={chip}>{company}</span>
                    </div>
                </header>

                <div className="mp__head" style={{ background: c.surface, borderColor: c.border }}>
                    <div className="mp__head-in">
                        <div className="mp__brand">
                            <span style={{ color: c.primary }}>BUS<b style={{ color: c.accent }}>IN</b>ESS</span>
                            <span style={{ color: c.primary }}>ESBJERG</span>
                        </div>
                        <div>
                            <h1>Overblik</h1>
                            <p style={{ color: c.muted }}>{company}</p>
                        </div>
                        <p className="mp__intro" style={{ color: c.muted }}>
                            Fra overblikket kan du nemt navigere mellem de forskellige funktioner.
                        </p>
                    </div>
                </div>

                <main className="mp__grid">
                    <a href="/anmod-ferie" className="mp__tile" style={tile}>
                        <span style={{ color: c.accent }}><CalPlusIcon size={52} stroke={0.9} /></span>
                        <span className="mp__tile-title">Anmod om ferie</span>
                        <span className="mp__tile-sub" style={{ color: c.muted }}>Send en ferieanmodning til din leder</span>
                    </a>

                    <button type="button" className="mp__tile" style={tile} onClick={() => listDialog.current?.showModal()}>
                        <span style={{ color: c.primary }}><ListIcon size={52} stroke={0.9} /></span>
                        <span className="mp__tile-title">Mine anmodninger</span>
                        <span className="mp__tile-sub" style={{ color: c.muted }}>Se status på dine ferieanmodninger</span>
                    </button>

                    <Link to="/kalender" className="mp__tile" style={tile}>
                        <span style={{ color: c.primary }}><CalIcon size={52} stroke={0.9} /></span>
                        <span className="mp__tile-title">Fælles kalender</span>
                        <span className="mp__tile-sub" style={{ color: c.muted }}>Se hvem der har ferie og fravær</span>
                    </Link>

                    <a href="/ferie" className="mp__tile" style={tile}>
                        <span style={{ color: c.primary }}><PlaneIcon size={52} stroke={0.9} /></span>
                        <span className="mp__tile-title">Ferie og fravær</span>
                        <span className="mp__tile-sub" style={{ color: c.muted }}>Administration af ferie og fravær</span>
                    </a>
                </main>
            </div>

            <dialog ref={listDialog} className="mp__dialog" style={tile} aria-labelledby="list-title">
                <div className="mp__dialog-head">
                    <h2 id="list-title">Mine anmodninger</h2>
                    <button type="button" className="mp__chip" style={chip} onClick={() => listDialog.current?.close()}>Luk</button>
                </div>
                {requests.length === 0 ? (
                    <p style={{ color: c.muted }}>Du har ingen anmodninger endnu.</p>
                ) : (
                    <ul className="mp__list">
                        {requests.map((r) => (
                            <li key={r.id} style={{ borderColor: c.border }}>
                                <span>{fmt(r.from)} – {fmt(r.to)} ({r.days} {r.days === 1 ? "dag" : "dage"})</span>
                                <span className="mp__status" style={{ background: c.accentSoft, color: c.accent }}>{r.status}</span>
                            </li>
                        ))}
                    </ul>
                )}
            </dialog>
        </div>
    );
}

export default MainPage;