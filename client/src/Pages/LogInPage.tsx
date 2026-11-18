import { useState, type FormEvent } from "react";
import "../index.css";
import {COLOURS} from "@/Colours.tsx";

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

    async function login(username: string, password: string): Promise<string | null> {
        const res = await fetch("/api/auth/login", {
            method: "POST",
            headers: {"Content-Type": "application/json"},
            body: JSON.stringify({username, password}),
        });

        if (!res.ok) return await res.text();

        const auth = await res.json();
        localStorage.setItem("absencePlanning_user", auth.userName);
        localStorage.setItem("absencePlanning_user_id", String(auth.id));
        localStorage.setItem("absencePlanning_token", auth.token);
        return null;
    }
    
  return (
    <main className="login" style={{background: COLOURS.bg}}>
      <section className="login__container">
        <h1 id="login-title" className="login__title" style={{color: COLOURS.text}}>Login</h1>
        <p className="login__sub" style={{color: COLOURS.text}}>Log in or register</p>

        <form noValidate>
          <label className="login__field" style={{color: COLOURS.text}}>
            Username
            <input
              type="username"
              autoComplete="username"
              required
              style={{background: COLOURS.surface,
                borderColor: COLOURS.border,
                color: COLOURS.text}}
            />
          </label>

          <label className="login__field" style={{color: COLOURS.text}}>
            Password
            <input
                type="password"
                autoComplete="current-password"
                required
                style={{background: COLOURS.surface,
                  borderColor: COLOURS.border,
                  color: COLOURS.text}}
            />
          </label>

          <button className="login__btn" type="submit" style={{color: COLOURS.text}}>Log In</button>
        </form>
      </section>

    </main>
  );
}

export default LogIn;
