import type {
    AuthenticateResponseDto,
    FilmPublicDto,
    FilmCopyDto,
} from "./types";

const BASE_URL = "http://localhost:5254";

function getToken(): string {
    return localStorage.getItem("token") ?? "";
}

function authHeaders(): HeadersInit {
    const token = getToken();
    return token ? { Authentication: token } : {};
}

export async function authenticate(username: string, password: string) {
    const res = await fetch(`${BASE_URL}/api/users/authenticate`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            Accept: "application/json",
        },
        body: JSON.stringify({ username, password }),
    });

    if (!res.ok) {
        throw new Error(`Auth failed: ${res.status}`);
    }

    const data = (await res.json()) as AuthenticateResponseDto;
    return data;
}

export async function getFilmsPublic() {
    const res = await fetch(`${BASE_URL}/api/films`, {
        headers: { Accept: "application/json" },
    });

    if (!res.ok) throw new Error(`Get films failed: ${res.status}`);
    return (await res.json()) as FilmPublicDto[];
}


export async function rentFilm(filmId: number, studioId: number) {
    const url = `${BASE_URL}/api/films/rent?id=${filmId}&studioid=${studioId}`;
    const res = await fetch(url, {
        method: "POST",
        headers: { Accept: "application/json", ...authHeaders() },
    });

    if (!res.ok) {
        throw new Error(`Rent failed: ${res.status}`);
    }
}

export async function returnFilm(filmId: number, studioId: number) {
    const url = `${BASE_URL}/api/films/return?id=${filmId}&studioid=${studioId}`;
    const res = await fetch(url, {
        method: "POST",
        headers: { Accept: "application/json", ...authHeaders()},
    });

    if (!res.ok) {
        throw new Error(`Return failed: ${res.status}`);
    }
}

export async function getMyRentals() {
    const res = await fetch(`${BASE_URL}/api/mystudio/rentals`, {
        headers: { Accept: "application/json", ...authHeaders() },
    });

    if (!res.ok) {
        throw new Error(`Get rentals failed: ${res.status}`);
    }

    return (await res.json()) as FilmCopyDto[];
}

export async function logout() {
    const res = await fetch(`${BASE_URL}/api/users/logout`, {
        method: "POST",
        headers: { Accept: "application/json", ...authHeaders()},
    });

    if (!res.ok) {
        throw new Error(`Logout failed: ${res.status}`);
    }
}

export async function registerFilmStudio(
    name: string,
    city: string,
    username: string,
    password: string
) {
    const res = await fetch(`${BASE_URL}/api/filmstudio/register`, {
    method: "POST",
    headers: {
        "Content-Type": "application/json",
        Accept: "application/json",
    },
    body: JSON.stringify({ name, city, username, password }),
    });

    if (!res.ok) {
    throw new Error(`Register failed: ${res.status}`);
    }

    return res.json();
}