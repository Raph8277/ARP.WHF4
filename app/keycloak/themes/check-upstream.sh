#!/usr/bin/env sh
# Vérifie, après une mise à jour de Keycloak, que le gabarit parent de login.ftl n'a pas changé.
# Le thème wfrp4 surcharge login.ftl (copie de keycloak.v2 + lien « Créer un compte ») : si le
# gabarit d'origine évolue, il faut reporter ces changements dans wfrp4/login/login.ftl.
#
# Usage (depuis app/) : sh keycloak/themes/check-upstream.sh [conteneur]
# Code de sortie : 0 identique, 1 différent (diff affiché), 2 erreur.
set -eu

CONTENEUR="${1:-wfrp4-keycloak}"
ICI="$(cd "$(dirname "$0")" && pwd)"
TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

JAR="$(MSYS_NO_PATHCONV=1 docker exec "$CONTENEUR" sh -c 'ls /opt/keycloak/lib/lib/main/org.keycloak.keycloak-themes-*.jar' | tr -d '\r')" || exit 2
VERSION="$(basename "$JAR" .jar | sed 's/org.keycloak.keycloak-themes-//')"
MSYS_NO_PATHCONV=1 docker exec "$CONTENEUR" cat "$JAR" > "$TMP/themes.jar"
( cd "$TMP" && unzip -oq themes.jar 'theme/keycloak.v2/login/login.ftl' ) || exit 2

REFERENCE="$(ls "$ICI"/upstream/keycloak.v2-login-*.ftl | tail -1)"
echo "Keycloak installé : $VERSION — référence : $(basename "$REFERENCE")"

if diff -u "$REFERENCE" "$TMP/theme/keycloak.v2/login/login.ftl"; then
    echo "OK : le gabarit parent est identique, wfrp4/login/login.ftl reste à jour."
else
    echo "ATTENTION : le gabarit parent a changé. Reporter ces différences dans wfrp4/login/login.ftl,"
    echo "puis remplacer la référence par upstream/keycloak.v2-login-$VERSION.ftl."
    exit 1
fi
