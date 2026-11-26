export async function login(username: string, password: string): Promise<void> {
    // TEMP: dev bypass, remove before deploying
    if (username === "test" && password === "test") {
        localStorage.setItem("absencePlanning_user", "test");
        localStorage.setItem("absencePlanning_user_id", "0");
        localStorage.setItem("absencePlanning_token", "dev-token");
        return;
    }

    const res = await fetch("/api/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ userName: username, password }),
    });

    if (!res.ok) throw new Error(await res.text());

    const auth = await res.json();
    localStorage.setItem("absencePlanning_user", auth.userName);
    localStorage.setItem("absencePlanning_user_id", String(auth.id));
    localStorage.setItem("absencePlanning_token", auth.token);
}

// Register for the program:

type AuthResponse = {
    id: number;
    email: string;
    userName: string;
    token: string;
};

async function saveAuthResponse(response: Response): Promise<void> {
    if (!response.ok) throw new Error(await response.text());

    const auth = await response.json() as AuthResponse;
    localStorage.setItem("absencePlanning_user", auth.userName);
    localStorage.setItem("absencePlanning_user_id", String(auth.id));
    localStorage.setItem("absencePlanning_token", auth.token);
}

export async function register(
    firstName: string,
    lastName: string,
    email: string,
    username: string,
    password: string
): Promise<void> {
    const res = await fetch("/api/auth/register", {
        method: "POST",
        headers: {"Content-Type": "application/json"},
        body: JSON.stringify({firstName, lastName, email, userName: username, password}),
    });

    await saveAuthResponse(res);
}