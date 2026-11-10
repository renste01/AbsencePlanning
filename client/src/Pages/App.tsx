import { useState, type FormEvent } from "react";
import "../index.css";
import {COLOURS} from "@/Colours.tsx";


export function LogIn() {
  return (
    <main className="login" style={{background: COLOURS.bg}}>
      <section className="login__container">
        <h1 id="login-title" className="login__title" style={{color: COLOURS.text}}>Login</h1>
        <p className="login__sub" style={{color: COLOURS.text}}>Log in or register</p>

        <form noValidate>
          <label className="login__field" style={{color: COLOURS.text}}>
            Email
            <input
              type="email"
              autoComplete="email"
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

        <p className="login__alt" style={{color: COLOURS.text}}>
          New here?
          <button type="button" style={{color: COLOURS.text}}>
            Create an account
          </button>
        </p>
      </section>

    </main>
  );
}

export default LogIn;
