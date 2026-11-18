import { useState, type ReactNode } from "react";
import { BE_COLOURS as COLOURS } from "@/Colours.tsx";
import "../index.css";

type PageShellProps = {
    title: string;
    subtitle: string;
    intro: string;
    children: ReactNode;
};

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
const ClockIcon = (p: { size?: number; stroke?: number }) => <Icon {...p}><circle cx="12" cy="12" r="9" /><path d="M12 7v5l3 2" /></Icon>;
const HelpIcon = (p: { size?: number }) => <Icon {...p}><circle cx="12" cy="12" r="9" /><path d="M9.5 9.5a2.5 2.5 0 1 1 3.5 2.3c-.6.3-1 .8-1 1.5M12 17h.01" /></Icon>;

export function PageShell({ title, subtitle, intro, children }: PageShellProps) {
    const company = "Din virksomhed";
    const [collapsed, setCollapsed] = useState(false);

    const c = COLOURS;
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
                        <a href="/" style={{ color: c.text }}>Overblik</a>
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
                            <h1>{title}</h1>
                            <p style={{ color: c.muted }}>{subtitle}</p>
                        </div>
                        <p className="mp__intro" style={{ color: c.muted }}>{intro}</p>
                    </div>
                </div>

                {children}
            </div>
        </div>
    );
}

export default PageShell;