export type Role = "admin" | "filmstudio" | string;

export type FilmStudioSummaryDto = {
    filmStudioId: number;
    name: string;
    city: string;
};

export type AuthenticatedUserDto = {
    userId: number;
    username: string;
    role: Role;
    filmStudioId: number | null;
    filmStudio: FilmStudioSummaryDto | null;
};

export type AuthenticateResponseDto = {
    token: string;
    user: AuthenticatedUserDto;
};

export type FilmCopyDto = {
    filmCopyId: number;
    filmId: number;
    rentedByFilmStudioId: number | null;
};

export type FilmPublicDto = {
    filmId: number;
    title: string;
    releaseYear: number;
};

export type FilmAuthDto = FilmPublicDto & {
    filmCopies: FilmCopyDto[];
};