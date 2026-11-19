import { useState, type FormEvent } from "react";
import "../index.css";
import { BE_COLOURS as COLOURS } from "@/Colours.tsx";

type LoginProps = {
    onLogin: (username: string, password: string) => Promise<void>;
    onSwitchToRegister: () => void;
};

export function LogIn({ onLogin, onSwitchToRegister }: LoginProps) 
{
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [busy, setBusy] = useState(false);

    async function handleSubmit(e: FormEvent) {
        e.preventDefault();
        setError(null);
        setBusy(true);
        try {
            await onLogin(username.trim(), password);
        } catch (err) {
            setError(
                err instanceof Error && err.message
                    ? err.message
                    : "We couldn't sign you in. Check your username and password."
            );
        } finally {
            setBusy(false);
        }
    }

    const c = COLOURS;
    
  return (
    <main className="login" style={{background: c.bg}}>
      <section className="login__container">
        <h1 id="login-title" className="login__title" style={{color: c.text}}>Login</h1>
        <p className="login__sub" style={{color: c.text}}>Log in or register</p>

        <form noValidate onSubmit={handleSubmit}>
          <label className="login__field" style={{color: c.text}}>
            Username
            <input
              type="text"
              autoComplete="username"
              required
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              style={{background: c.surface,
                borderColor: c.border,
                color: c.text}}
            />
          </label>

          <label className="login__field" style={{color: c.text}}>
            Password
            <input
                type="password"
                autoComplete="current-password"
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                style={{background: c.surface,
                  borderColor: c.border,
                  color: c.text}}
            />
          </label>

          <button className="login__btn" type="submit" disabled={busy} style={{color: c.surface}}> 
              {busy ? "Logging in..." : "Log In"}
          
          </button>
        </form>
      </section>

    </main>
  );
}

export default LogIn;
