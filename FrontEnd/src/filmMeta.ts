export type FilmMeta = {
    title: string;
    description: string;
    posterUrl: string;
};

const metaByTitle: Record<string, FilmMeta> = {
    "the matrix": {
        title: "The Matrix",
        description:
            "En hacker upptäcker att verkligheten är en simulering och dras in i en kamp mot maskinerna.",
        posterUrl: "/posters/thematrix1999.jpg",
    },
    "inception": {
        title: "Inception",
        description:
            "Ett team tar sig in i drömmar för att plantera en idé – men gränsen mellan dröm och verklighet suddas ut.",
        posterUrl: "/posters/inception2010.jpg",
    },
    "interstellar": {
        title: "Interstellar",
        description:
            "En expedition reser genom ett maskhål för att hitta mänsklighetens nästa hem.",
        posterUrl: "/posters/interstellar.jpg",
    },
    "the dark knight": {
        title: "The Dark Knight",
        description:
            "Batman ställs mot Jokern i en kamp om Gotham och moralens gränser.",
        posterUrl: "/posters/dark-knight.jpg",
    },
    "pulp fiction": {
        title: "Pulp Fiction",
        description:
            "Kriminella livsöden vävs samman i en stiliserad berättelse i Los Angeles.",
        posterUrl: "/posters/pulp-fiction.jpg",
    },
    "parasite": {
        title: "Parasite",
        description:
            "Två familjer från olika samhällsklasser dras in i en oväntad och mörk spiral.",
        posterUrl: "/posters/parasite.jpg",
    },
    "fight club": {
        title: "Fight Club",
        description:
            "Fight Club (1999): En desillusionerad kontorsarbetare och en karismatisk främling startar en hemlig fight club som snabbt växer till något mycket farligare.",
        posterUrl: "/posters/fight-club.jpg",
    },
    "the godfather": {
        title: "The Godfather",
        description:
            "The Godfather (1972): En maffiapatriark försöker skydda sin familj och sitt imperium, medan hans son dras in i en värld han först vill undvika.",
        posterUrl: "/posters/godfather.jpg",
    },
    "gladiator": {
        title: "Gladiator",
        description:
            "Gladiator (2000): En förrådd romersk general tvingas bli gladiator och söker hämnd i arenan mot den makthungrige kejsaren.",
        posterUrl: "/posters/gladiator.jpg",
    },
    "whiplash": {
        title: "Whiplash",
        description:
            "Whiplash (2014): En ambitiös jazztrummis pressas till bristningsgränsen av en skoningslös lärare i jakt på perfektion.",
        posterUrl: "/posters/whiplash.jpg",
    },
    "se7en": {
        title: "Se7en",
        description:
            "Se7en (1995): Två poliser jagar en seriemördare vars brott är inspirerade av de sju dödssynderna, och fallet mörknar för varje ledtråd.",
        posterUrl: "/posters/seven.jpg",
    },
    "bad boys": {
        title: "Bad Boys",
        description:
            "Bad Boys (1995): Två Miami-poliser med helt olika stil måste samarbeta för att stoppa en stulen heroinleverans, samtidigt som en viktig vittne behöver skyddas och kaoset eskalerar.",
        posterUrl: "/posters/bad-boys.jpg",
    }
};

function normalizeTitleKey(title: string) {
    return title.trim().toLowerCase();
}

export function getFilmMeta(title: string) {
    return metaByTitle[normalizeTitleKey(title)] ?? null;
}