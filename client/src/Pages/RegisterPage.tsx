import { useState, type FormEvent } from "react";
import { BE_COLOURS as COLOURS } from "@/Colours.tsx";
import "../index.css";

type RegisterPageProps = {
    onRegister: (firstName: string, lastName: string, email: string, username: string, password: string) => Promise<void>;
    onSwitchToLogin: () => void;
};

export function RegisterPage({ onRegister, onSwitchToLogin }: RegisterPageProps) {
    const [firstName, setFirstName] = useState("");
    const [lastName, setLastName] = useState("");
    const [email, setEmail] = useState("");
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [busy, setBusy] = useState(false);
    const c = COLOURS;

    async function handleSubmit(event: FormEvent) {
        event.preventDefault();
        setError(null);
        setBusy(true);

        try {
            await onRegister(firstName.trim(), lastName.trim(), email.trim(), username.trim(), password);
        } catch (err) {
            setError(err instanceof Error ? err.message : "Registreringen mislykkedes.");
        } finally {
            setBusy(false);
        }
    }

    return (
        <main className="login" style={{ background: c.bg }}>
            <section className="login__container">
                <h1 className="login__title" style={{ color: c.text }}>Opret bruger</h1>
                <p className="login__sub" style={{ color: c.text }}>Opret din medarbejderkonto.</p>

                <form noValidate onSubmit={handleSubmit}>
                    <label className="login__field" style={{ color: c.text }}>
                        Fornavn
                        <input
                            type="text"
                            required
                            value={firstName}
                            onChange={(event) => setFirstName(event.target.value)}
                            style={{ background: c.surface, borderColor: c.border, color: c.text }}
                        />
                    </label>

                    <label className="login__field" style={{ color: c.text }}>
                        Efternavn
                        <input
                            type="text"
                            required
                            value={lastName}
                            onChange={(event) => setLastName(event.target.value)}
                            style={{ background: c.surface, borderColor: c.border, color: c.text }}
                        />
                    </label>

                    <label className="login__field" style={{ color: c.text }}>
                        Email
                        <input
                            type="email"
                            autoComplete="email"
                            required
                            value={email}
                            onChange={(event) => setEmail(event.target.value)}
                            style={{ background: c.surface, borderColor: c.border, color: c.text }}
                        />
                    </label>

                    <label className="login__field" style={{ color: c.text }}>
                        Brugernavn
                        <input
                            type="text"
                            autoComplete="username"
                            required
                            value={username}
                            onChange={(event) => setUsername(event.target.value)}
                            style={{ background: c.surface, borderColor: c.border, color: c.text }}
                        />
                    </label>

                    <label className="login__field" style={{ color: c.text }}>
                        Adgangskode
                        <input
                            type="password"
                            autoComplete="new-password"
                            minLength={8}
                            required
                            value={password}
                            onChange={(event) => setPassword(event.target.value)}
                            style={{ background: c.surface, borderColor: c.border, color: c.text }}
                        />
                    </label>

                    {error && <p role="alert">{error}</p>}
                    <button className="login__btn" type="submit" disabled={busy} style={{ color: c.surface }}>
                        {busy ? "Opretter..." : "Opret bruger"}
                    </button>
                </form>

                <button type="button" onClick={onSwitchToLogin}>Tilbage til login</button>
            </section>
        </main>
    );
}
