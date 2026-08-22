#!/usr/bin/env sh
set -eu

KC="/opt/keycloak/bin/kcadm.sh"
REALM="${WFRP4_KEYCLOAK_REALM:-wfrp4}"

is_configured() {
  client_id="$1"
  client_secret="$2"

  if [ -n "$client_id" ] && [ -n "$client_secret" ]; then
    echo true
  else
    echo false
  fi
}

GOOGLE_ENABLED="$(is_configured "${WFRP4_GOOGLE_CLIENT_ID:-}" "${WFRP4_GOOGLE_CLIENT_SECRET:-}")"
YAHOO_ENABLED="$(is_configured "${WFRP4_YAHOO_CLIENT_ID:-}" "${WFRP4_YAHOO_CLIENT_SECRET:-}")"
META_ENABLED="$(is_configured "${WFRP4_META_CLIENT_ID:-}" "${WFRP4_META_CLIENT_SECRET:-}")"

"$KC" config credentials \
  --server http://localhost:8080 \
  --realm master \
  --user "${KEYCLOAK_ADMIN:-admin}" \
  --password "${KEYCLOAK_ADMIN_PASSWORD:-admin}"

upsert_provider() {
  alias="$1"
  shift

  if "$KC" get "identity-provider/instances/$alias" -r "$REALM" >/dev/null 2>&1; then
    "$KC" update "identity-provider/instances/$alias" -r "$REALM" "$@"
  else
    "$KC" create identity-provider/instances -r "$REALM" "$@"
  fi
}

create_mapper_if_missing() {
  alias="$1"
  mapper_name="$2"
  file="$3"

  if "$KC" get "identity-provider/instances/$alias/mappers" -r "$REALM" | grep -q "\"name\" : \"$mapper_name\""; then
    echo "Mapper already exists: $mapper_name"
  else
    "$KC" create "identity-provider/instances/$alias/mappers" -r "$REALM" -f "$file"
  fi
}

upsert_provider google \
  -s alias=google \
  -s displayName=Google \
  -s providerId=google \
  -s enabled="$GOOGLE_ENABLED" \
  -s trustEmail=true \
  -s storeToken=false \
  -s addReadTokenRoleOnCreate=false \
  -s authenticateByDefault=false \
  -s linkOnly=false \
  -s 'firstBrokerLoginFlowAlias=first broker login' \
  -s "config.clientId=${WFRP4_GOOGLE_CLIENT_ID:-}" \
  -s "config.clientSecret=${WFRP4_GOOGLE_CLIENT_SECRET:-}" \
  -s 'config.defaultScope=openid profile email' \
  -s config.syncMode=IMPORT

upsert_provider yahoo \
  -s alias=yahoo \
  -s displayName=Yahoo \
  -s providerId=oidc \
  -s enabled="$YAHOO_ENABLED" \
  -s trustEmail=true \
  -s storeToken=false \
  -s addReadTokenRoleOnCreate=false \
  -s authenticateByDefault=false \
  -s linkOnly=false \
  -s 'firstBrokerLoginFlowAlias=first broker login' \
  -s "config.clientId=${WFRP4_YAHOO_CLIENT_ID:-}" \
  -s "config.clientSecret=${WFRP4_YAHOO_CLIENT_SECRET:-}" \
  -s config.authorizationUrl=https://api.login.yahoo.com/oauth2/request_auth \
  -s config.tokenUrl=https://api.login.yahoo.com/oauth2/get_token \
  -s config.userInfoUrl=https://api.login.yahoo.com/openid/v1/userinfo \
  -s config.issuer=https://api.login.yahoo.com \
  -s config.jwksUrl=https://api.login.yahoo.com/openid/v1/certs \
  -s config.clientAuthMethod=client_secret_basic \
  -s 'config.defaultScope=openid profile email' \
  -s config.syncMode=IMPORT \
  -s config.validateSignature=true \
  -s config.useJwksUrl=true \
  -s config.backchannelSupported=false

upsert_provider meta \
  -s alias=meta \
  -s displayName=Meta \
  -s providerId=facebook \
  -s enabled="$META_ENABLED" \
  -s trustEmail=true \
  -s storeToken=false \
  -s addReadTokenRoleOnCreate=false \
  -s authenticateByDefault=false \
  -s linkOnly=false \
  -s 'firstBrokerLoginFlowAlias=first broker login' \
  -s "config.clientId=${WFRP4_META_CLIENT_ID:-}" \
  -s "config.clientSecret=${WFRP4_META_CLIENT_SECRET:-}" \
  -s 'config.defaultScope=email public_profile' \
  -s config.syncMode=IMPORT

create_mapper_if_missing google google-wfrp4-joueur /tmp/wfrp4-social-idps/google-role-mapper.json
create_mapper_if_missing yahoo yahoo-wfrp4-joueur /tmp/wfrp4-social-idps/yahoo-role-mapper.json
create_mapper_if_missing meta meta-wfrp4-joueur /tmp/wfrp4-social-idps/meta-role-mapper.json

"$KC" get identity-provider/instances -r "$REALM"

if [ "$GOOGLE_ENABLED" = false ]; then
  echo "Google disabled: WFRP4_GOOGLE_CLIENT_ID and/or WFRP4_GOOGLE_CLIENT_SECRET are missing."
fi

if [ "$YAHOO_ENABLED" = false ]; then
  echo "Yahoo disabled: WFRP4_YAHOO_CLIENT_ID and/or WFRP4_YAHOO_CLIENT_SECRET are missing."
fi

if [ "$META_ENABLED" = false ]; then
  echo "Meta disabled: WFRP4_META_CLIENT_ID and/or WFRP4_META_CLIENT_SECRET are missing."
fi
