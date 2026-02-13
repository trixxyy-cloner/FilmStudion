import { useEffect, useMemo, useState, type FormEvent } from "react";
import {
  authenticate,
  getFilmsPublic,
  getMyRentals,
  logout,
  registerFilmStudio,
  rentFilm,
  returnFilm,
} from "./api";
import { getFilmMeta } from "./filmMeta";
import "./App.css";

import type {
  AuthenticateResponseDto,
  AuthenticatedUserDto,
  FilmCopyDto,
  FilmPublicDto,
} from "./types";

type AuthState = {
  token: string;
  user: AuthenticatedUserDto;
};

type View = "home" | "films" | "film" | "rentals";
type AuthScreen = "none" | "login" | "register";

function loadAuth(): AuthState | null {
  const token = localStorage.getItem("token") ?? "";
  const userRaw = localStorage.getItem("user") ?? "";
  if (!token || !userRaw) return null;

  try {
    const user = JSON.parse(userRaw) as AuthenticatedUserDto;
    return { token, user };
  } catch {
    return null;
  }
}

function saveAuth(next: AuthState | null) {
  if (!next) {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    return;
  }

  localStorage.setItem("token", next.token);
  localStorage.setItem("user", JSON.stringify(next.user));
}

export default function App() {
  const [auth, setAuth] = useState<AuthState | null>(() => loadAuth());

  const [view, setView] = useState<View>("home");
  const [authScreen, setAuthScreen] = useState<AuthScreen>("none");

  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");

  const [menuOpen, setMenuOpen] = useState(false);

  const [studioName, setStudioName] = useState("");
  const [city, setCity] = useState("");

  const [selectedFilmId, setSelectedFilmId] = useState<number | null>(null);

  const [films, setFilms] = useState<FilmPublicDto[]>([]);
  const [rentals, setRentals] = useState<FilmCopyDto[]>([]);

  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");

  const isLoggedIn = auth !== null;
  const role = (auth?.user.role ?? "").toLowerCase();
  const isFilmStudio = isLoggedIn && role === "filmstudio";
  const studioId = auth?.user.filmStudioId ?? null;

  const myRentedFilmIds = useMemo(
    () => new Set(rentals.map((r) => r.filmId)),
    [rentals]
  );

  const filmTitleById = useMemo(() => {
    const map = new Map<number, string>();
    for (const film of films) map.set(film.filmId, film.title);
    return map;
  }, [films]);

  const selectedFilm = useMemo(
    () => films.find((f) => f.filmId === selectedFilmId) ?? null,
    [films, selectedFilmId]
  );

  const selectedMeta = selectedFilm ? getFilmMeta(selectedFilm.title) : null;
  const selectedAlreadyRentedByMe = selectedFilm
    ? myRentedFilmIds.has(selectedFilm.filmId)
    : false;

  function forceLogout() {
    saveAuth(null);
    setAuth(null);
    setRentals([]);
  }

  function userFriendlyError(e: unknown): string {
    const msg = e instanceof Error ? e.message : "";

    if (
      msg.includes("Rent failed: 401") ||
      msg.includes("Return failed: 401") ||
      msg.includes("Get rentals failed: 401")
    ) {
      return "Du har blivit utloggad. Logga in igen.";
    }

    if (msg.includes("Rent failed: 409")) return "Inga lediga kopior finns just nu.";
    if (msg.includes("Return failed: 409")) return "Du har ingen kopia att lämna tillbaka.";
    if (msg.includes("Get rentals failed")) return "Kunde inte hämta dina lån.";
    if (msg.includes("Get films failed")) return "Kunde inte hämta filmer.";
    if (msg.includes("Auth failed: 401")) return "Fel username eller password.";

    return "Något gick fel. Försök igen.";
  }

  function closeMenu() {
    setMenuOpen(false);
  }

  function resetAuthForm() {
  setUsername("");
  setPassword("");
  setStudioName("");
  setCity("");
  setError("");
}

  function openHome() {
    resetAuthForm();
    setAuthScreen("none");
    setView("home");
    closeMenu();
  }

  function openFilms() {
    resetAuthForm();
    setAuthScreen("none");
    setView("films");
    closeMenu();
  }

  function openRentals() {
    resetAuthForm();
    setAuthScreen("none");
    setView("rentals");
    closeMenu();
  }

  function openFilm(filmId: number) {
    resetAuthForm();
    setAuthScreen("none");
    setSelectedFilmId(filmId);
    setView("film");
    closeMenu();
  }

  async function refreshFilms() {
    setBusy(true);
    setError("");
    try {
      const data = await getFilmsPublic();
      setFilms(data);
      setSelectedFilmId((current) => {
        if (data.length === 0) return null;
        if (current == null) return data[0].filmId;
        return data.some((f) => f.filmId === current) ? current : data[0].filmId;
      });
    } catch (e) {
      setError(userFriendlyError(e));
    } finally {
      setBusy(false);
    }
  }

  async function refreshRentals() {
    if (!isFilmStudio) {
      setRentals([]);
      return;
    }

    setBusy(true);
    setError("");
    try {
      const data = await getMyRentals();
      setRentals(data);
    } catch (e) {
      setError(userFriendlyError(e));
      if (e instanceof Error && e.message.includes("401")) forceLogout();
    } finally {
      setBusy(false);
    }
  }

  useEffect(() => {
    refreshFilms();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (!isFilmStudio) {
      setRentals([]);
      return;
    }

    (async () => {
      setError("");
      try {
        const data = await getMyRentals();
        setRentals(data);
      } catch (e) {
        setError(userFriendlyError(e));
        if (e instanceof Error && e.message.includes("401")) forceLogout();
      }
    })();
  }, [isFilmStudio, studioId]);

  async function onSubmitAuth(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setBusy(true);
    setError("");

    try {
      if (authScreen === "register") {
        await registerFilmStudio(
          studioName.trim(),
          city.trim(),
          username.trim(),
          password
        );

        const res = await authenticate(username.trim(), password);
        const nextAuth: AuthState = { token: res.token, user: res.user };
        saveAuth(nextAuth);
        setAuth(nextAuth);

        setPassword("");
        setStudioName("");
        setCity("");
        setAuthScreen("none");
        setView("home");

        await refreshRentals();
        return;
      }

      const res = (await authenticate(
        username.trim(),
        password
      )) as AuthenticateResponseDto;
      const nextAuth: AuthState = { token: res.token, user: res.user };
      saveAuth(nextAuth);
      setAuth(nextAuth);

      setPassword("");
      setAuthScreen("none");
      setView("home");

      await refreshRentals();
    } catch (e) {
      setError(userFriendlyError(e));
    } finally {
      setBusy(false);
    }
  }

  async function onLogout() {
    setBusy(true);
    setError("");

    try {
      await logout();
    } catch (e) {
      setError(userFriendlyError(e));
    } finally {
      forceLogout();
      setAuthScreen("none");
      setView("home");
      setBusy(false);
      closeMenu();
    }
  }

  async function onRent(filmId: number) {
    if (!isFilmStudio || studioId == null) return;

    setBusy(true);
    setError("");

    try {
      await rentFilm(filmId, studioId);
      await refreshRentals();
    } catch (e) {
      setError(userFriendlyError(e));
      if (e instanceof Error && e.message.includes("401")) forceLogout();
    } finally {
      setBusy(false);
    }
  }

  async function onReturn(filmId: number) {
    if (!isFilmStudio || studioId == null) return;

    setBusy(true);
    setError("");

    try {
      await returnFilm(filmId, studioId);
      await refreshRentals();
    } catch (e) {
      setError(userFriendlyError(e));
      if (e instanceof Error && e.message.includes("401")) forceLogout();
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="app">
      <header className="nav">
        <div className="nav__inner">
          <div className="nav__left">
            <h1 className="nav__title">
              <span
              className="nav__titleLink"
              role="button"
              tabIndex={0}
              onClick={openHome}
              onKeyDown={(e) => {
                if (e.key === "Enter" || e.key === " ") openHome();
              }}
              aria-label="Hem"
              >
                FilmStudion
              </span>
            </h1>
          </div>

          <nav className="nav__center">
            <>
              <button className="nav__link" type="button" onClick={openHome}>
                Hem
              </button>

              <button className="nav__link" type="button" onClick={openFilms}>
                Alla filmer
              </button>

              {isFilmStudio ? (
                <button
                  className="nav__link"
                  type="button"
                  onClick={openRentals}
                >
                  Mina lånade filmer
                </button>
              ) : null}
            </>
          </nav>

          <div className="nav__right">
            {auth ? (
              <div className="navUser">
                <div className="navUser__meta">
                  <div>
                    Inloggad som <strong>{auth.user.username}</strong>
                  </div>
                  {auth.user.filmStudio ? (
                    <div className="muted">
                      {auth.user.filmStudio.name} ({auth.user.filmStudio.city})
                    </div>
                  ) : null}
                </div>

                <button
                  className="btn btn--secondary"
                  type="button"
                  onClick={onLogout}
                  disabled={busy}
                >
                  Logga ut
                </button>
              </div>
            ) : (
              <div className="navGuest">
                <button
                  className="btn btn--secondary"
                  type="button"
                  onClick={() => {
                    setError("");
                    setAuthScreen("login");
                    closeMenu();
                  }}
                  disabled={busy}
                >
                  Logga in
                </button>

                <button
                  className="btn"
                  type="button"
                  onClick={() => {
                    setError("");
                    setAuthScreen("register");
                    closeMenu();
                  }}
                  disabled={busy}
                >
                  Skapa konto
                </button>
              </div>
            )}
          </div>

          <button
            className="nav__burger"
            type="button"
            aria-label="Meny"
            aria-controls="nav-menu"
            aria-expanded={menuOpen}
            onClick={() => setMenuOpen((v) => !v)}
          >
            <span className="nav__burgerBars" aria-hidden="true" />
          </button>
        </div>

        <div
          id="nav-menu"
          className={`navMenu ${menuOpen ? "navMenu--open" : ""}`}
        >
          <>
            <button className="nav__link" type="button" onClick={openHome}>
              Hem
            </button>

            <button className="nav__link" type="button" onClick={openFilms}>
              Alla filmer
            </button>

            {isFilmStudio ? (
              <button className="nav__link" type="button" onClick={openRentals}>
                Mina lånade filmer
              </button>
            ) : null}

            <hr className="navMenu__sep" />

            {auth ? (
              <button
                className="btn btn--secondary"
                type="button"
                onClick={onLogout}
                disabled={busy}
              >
                Logga ut
              </button>
            ) : (
              <>
                <button
                  className="btn btn--secondary"
                  type="button"
                  onClick={() => {
                    resetAuthForm();
                    setAuthScreen("login");
                    closeMenu();
                  }}
                  disabled={busy}
                >
                  Logga in
                </button>

                <button
                  className="btn"
                  type="button"
                  onClick={() => {
                    resetAuthForm();
                    setAuthScreen("register");
                    closeMenu();
                  }}
                  disabled={busy}
                >
                  Skapa konto
                </button>
              </>
            )}
          </>
        </div>
      </header>

      <main className="content">
        {authScreen !== "none" ? (
          <section className="authPage">
            <div className="authCard">
              <div className="authCard__header">
                <h2 className="authCard__title">
                  {authScreen === "login" ? "Logga in" : "Skapa filmstudio-konto"}
                </h2>

                <button
                  className="btn btn--secondary"
                  type="button"
                  onClick={() => {
                    setError("");
                    setAuthScreen("none");
                  }}
                  disabled={busy}
                >
                  Tillbaka
                </button>
              </div>

              {authScreen === "register" ? (
                <form className="authForm" onSubmit={onSubmitAuth}>
                  <label className="field">
                    <span className="field__label">Studionamn</span>
                    <input
                      className="field__input"
                      value={studioName}
                      onChange={(e) => setStudioName(e.target.value)}
                    />
                  </label>

                  <label className="field">
                    <span className="field__label">Stad</span>
                    <input
                      className="field__input"
                      value={city}
                      onChange={(e) => setCity(e.target.value)}
                    />
                  </label>

                  <label className="field">
                    <span className="field__label">Username</span>
                    <input
                      className="field__input"
                      value={username}
                      onChange={(e) => setUsername(e.target.value)}
                      autoComplete="username"
                    />
                  </label>

                  <label className="field">
                    <span className="field__label">Password</span>
                    <input
                      className="field__input"
                      type="password"
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      autoComplete="new-password"
                    />
                  </label>

                  {error ? <div className="authCard__error">{error}</div> : null}

                  <button className="btn" type="submit" disabled={busy}>
                    Skapa konto
                  </button>
                </form>
              ) : (
                <form className="authForm" onSubmit={onSubmitAuth}>
                  <label className="field">
                    <span className="field__label">Username</span>
                    <input
                      className="field__input"
                      value={username}
                      onChange={(e) => setUsername(e.target.value)}
                      autoComplete="username"
                    />
                  </label>

                  <label className="field">
                    <span className="field__label">Password</span>
                    <input
                      className="field__input"
                      type="password"
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      autoComplete="current-password"
                    />
                  </label>

                  {error ? <div className="authCard__error">{error}</div> : null}

                  <button className="btn" type="submit" disabled={busy}>
                    Logga in
                  </button>
                </form>
              )}
            </div>
          </section>
        ) : (
          <>
            {view === "home" ? (
              <section className="section">
                <div className="section__header">
                  <h2 className="section__title">Populärt</h2>
                  <button
                    className="btn btn--secondary"
                    type="button"
                    onClick={refreshFilms}
                    disabled={busy}
                  >
                    {busy ? "Uppdaterar..." : "Uppdatera"}
                  </button>
                </div>

                {films.length === 0 ? (
                  <p className="muted">Inga filmer än.</p>
                ) : (
                  <>
                    <div className="filmBrowser">
                      <div className="filmCarousel">
                        {films.map((f) => {
                          const meta = getFilmMeta(f.title);
                          const isSelected = f.filmId === selectedFilmId;

                          return (
                            <button
                              key={f.filmId}
                              type="button"
                              className={`filmCard ${
                                isSelected ? "filmCard--selected" : ""
                              }`}
                              onClick={() => openFilm(f.filmId)}
                            >
                              {meta ? (
                                <img
                                  className="filmCard__poster"
                                  src={meta.posterUrl}
                                  alt={f.title}
                                />
                              ) : (
                                <div className="filmCard__poster filmCard__poster--empty" />
                              )}

                              <div className="filmCard__title">{f.title}</div>
                              <div className="muted">{f.releaseYear}</div>
                            </button>
                          );
                        })}
                      </div>
                    </div>

                    <button
                      className="btn btn--secondary"
                      type="button"
                      onClick={openFilms}
                    >
                      Visa alla filmer
                    </button>

                    {isFilmStudio ? (
                      <button
                        className="btn btn--secondary"
                        type="button"
                        onClick={openRentals}
                      >
                        Mina lånade filmer
                      </button>
                    ) : null}
                  </>
                )}

                {error ? <div className="alert">{error}</div> : null}
              </section>
            ) : null}

            {view === "films" ? (
              <section className="section">
                <div className="section__header">
                  <h2 className="section__title">Alla filmer</h2>
                  <div className="section__headerActions">
                    <button
                      className="btn btn--secondary"
                      type="button"
                      onClick={refreshFilms}
                      disabled={busy}
                    >
                      {busy ? "Uppdaterar..." : "Uppdatera"}
                    </button>
                  </div>
                </div>

                {error ? <div className="alert">{error}</div> : null}

                
                <ul className="posterGrid">
                  {films.map((f) => {
                    const meta = getFilmMeta(f.title);

                    return (
                      <li key={f.filmId}>
                        <button
                        type="button"
                        className="posterTile"
                        onClick={() => openFilm(f.filmId)}
                        aria-label={`${f.title} (${f.releaseYear})`}
                        >
                          {meta ? (
                            <img className="posterTile__img" src={meta.posterUrl} alt={f.title}></img>
                          ) : (
                            <div className="posterTile__img posterTile__img--empty"></div>
                          )}
                        </button>
                      </li>
                    )
                  })}
                </ul>

              </section>
            ) : null}

            {view === "film" ? (
              <section className="section">
                <div className="section__header">
                  <h2 className="section__title">Film</h2>
                  <button
                    className="btn btn--secondary"
                    type="button"
                    onClick={openFilms}
                  >
                    Tillbaka
                  </button>
                </div>

                {error ? <div className="alert">{error}</div> : null}

                {selectedFilm ? (
                  <div className="filmDetails">
                    <div className="filmDetails__header">
                      <h3 className="filmDetails__title">
                        {selectedFilm.title}{" "}
                        <span className="muted">
                          ({selectedFilm.releaseYear})
                        </span>
                      </h3>

                      {isFilmStudio ? (
                        <div className="filmDetails__actions">
                          <button
                            className="btn"
                            onClick={() => onRent(selectedFilm.filmId)}
                            disabled={busy || selectedAlreadyRentedByMe}
                          >
                            Hyr
                          </button>

                          <button
                            className="btn btn--secondary"
                            onClick={() => onReturn(selectedFilm.filmId)}
                            disabled={busy || !selectedAlreadyRentedByMe}
                          >
                            Lämna tillbaka
                          </button>
                        </div>
                      ) : (
                        <div className="muted">
                          Logga in som filmstudio för att hyra/lämna tillbaka.
                        </div>
                      )}
                    </div>

                    {selectedMeta ? (
                      <div className="filmDetails__body">
                        <img
                          className="filmDetails__poster"
                          src={selectedMeta.posterUrl}
                          alt={selectedFilm.title}
                        />
                        <p className="filmDetails__description">
                          {selectedMeta.description}
                        </p>
                      </div>
                    ) : (
                      <p className="muted">
                        Ingen beskrivning/bild än för denna film.
                      </p>
                    )}
                  </div>
                ) : (
                  <p className="muted">Ingen film vald.</p>
                )}
              </section>
            ) : null}

            {view === "rentals" ? (
              <section className="section">
                <div className="section__header">
                  <h2 className="section__title">Mina lånade filmer</h2>
                  <button
                    className="btn btn--secondary"
                    type="button"
                    onClick={refreshRentals}
                    disabled={busy}
                  >
                    {busy ? "Uppdaterar..." : "Uppdatera"}
                  </button>
                </div>

                {error ? <div className="alert">{error}</div> : null}

                {rentals.length === 0 ? (
                  <p className="muted">Du har inga lån.</p>
                ) : (
                  <ul className="list">
                    {rentals.map((r) => (
                      <li key={r.filmCopyId} className="rentalRow">
                        <button
                          className="rentalRow__button"
                          type="button"
                          onClick={() => openFilm(r.filmId)}
                        >
                          <div className="rentalRow__title">
                            {filmTitleById.get(r.filmId) ?? `Film ${r.filmId}`}
                          </div>
                          <div className="muted">Klicka för detaljer</div>
                        </button>
                      </li>
                    ))}
                  </ul>
                )}
              </section>
            ) : null}
          </>
        )}
      </main>
    </div>
  );
}

