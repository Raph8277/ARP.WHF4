# HowToConnectYahoo

Guide de configuration de la connexion Yahoo via Keycloak pour WFRP4.

## Principe

L'application Blazor ne parle pas directement a Yahoo. Elle redirige vers Keycloak, puis Keycloak agit comme broker OIDC generique avec Yahoo (provider `oidc`, alias `yahoo`).

En developpement local :
- application WFRP4 : `http://localhost:5080`
- Keycloak : `http://localhost:8080`
- realm : `wfrp4`
- provider Keycloak : `yahoo`

L'URL de callback Yahoo est donc :

```text
http://localhost:8080/realms/wfrp4/broker/yahoo/endpoint
```

Cette URL ne doit pas etre ouverte directement dans le navigateur. Elle sert uniquement a Yahoo pour revenir vers Keycloak apres authentification.

## 1. Creer une application Yahoo

Dans le Yahoo Developer Network (`https://developer.yahoo.com/apps/`) :

1. Se connecter avec un compte Yahoo.
2. Cliquer **Create an App**.
3. Nommer l'application, par exemple `WFRP4 local`.
4. Dans **Permissions API**, choisir **OpenID Connect** et selectionner au minimum `profile` et `email`.
5. Dans **Redirect URI(s)**, ajouter exactement :

```text
http://localhost:8080/realms/wfrp4/broker/yahoo/endpoint
```

6. Enregistrer l'application.

Yahoo fournit ensuite :
- un **Client ID (Consumer Key)**
- un **Client Secret (Consumer Secret)**

Ces deux valeurs restent consultables sur `https://developer.yahoo.com/apps/`, en ouvrant l'application creee ; le Client Secret peut y etre regenere si necessaire.

## 2. Configurer les variables locales

Creer le fichier local `app/.env` a partir de `app/.env.example`.

```env
WFRP4_GOOGLE_CLIENT_ID=
WFRP4_GOOGLE_CLIENT_SECRET=

WFRP4_YAHOO_CLIENT_ID=xxxxx
WFRP4_YAHOO_CLIENT_SECRET=xxxxx

WFRP4_META_CLIENT_ID=
WFRP4_META_CLIENT_SECRET=

WFRP4_MS_CLIENT_ID=
WFRP4_MS_CLIENT_SECRET=
```

Le fichier `app/.env` contient des secrets et doit rester ignore par Git.

## 3. Appliquer la configuration Keycloak

Depuis `app/` :

```powershell
docker compose up -d keycloak
docker cp keycloak/social-idps/. wfrp4-keycloak:/tmp/wfrp4-social-idps
docker exec wfrp4-keycloak sh /tmp/wfrp4-social-idps/apply-social-idps.sh
```

Le script active Yahoo seulement si `WFRP4_YAHOO_CLIENT_ID` et `WFRP4_YAHOO_CLIENT_SECRET` sont renseignes. Google, Meta et Microsoft suivent la meme regle independamment.

Pour verifier l'etat du provider Yahoo :

```powershell
docker exec wfrp4-keycloak /opt/keycloak/bin/kcadm.sh config credentials --server http://localhost:8080 --realm master --user admin --password admin
docker exec wfrp4-keycloak /opt/keycloak/bin/kcadm.sh get identity-provider/instances/yahoo -r wfrp4 --fields alias,enabled
```

Resultat attendu :

```json
{
  "alias": "yahoo",
  "enabled": true
}
```

## 4. Tester la connexion

Ouvrir l'application :

```text
http://localhost:5080
```

Puis cliquer sur la connexion et choisir **Yahoo** dans l'ecran Keycloak.

Ne pas ouvrir directement :

```text
http://localhost:8080/realms/wfrp4/broker/yahoo/endpoint
```

Cette URL est uniquement le callback technique.

## Depannage

### Erreur Yahoo `invalid_redirect_uri` ou page blanche apres connexion

Yahoo n'a pas l'URI exacte envoyee par Keycloak.

Verifier dans le Yahoo Developer Network, sur l'application, que l'URI suivante est dans **Redirect URI(s)** :

```text
http://localhost:8080/realms/wfrp4/broker/yahoo/endpoint
```

Points a respecter :
- `http`, pas `https`, en local
- `localhost`, pas `127.0.0.1`
- port `8080`
- pas de `/` final

### Erreur Keycloak `Unexpected error when authenticating with identity provider`

Le provider Yahoo est actif mais le `clientId` ou le `clientSecret` est incorrect, ou l'application Yahoo n'a pas la permission OpenID Connect activee.

Verifier dans le Yahoo Developer Network que **OpenID Connect** est bien coche dans les permissions de l'application, puis verifier `app/.env`.

### Erreur Keycloak `Missing state parameter`

Le callback Keycloak a ete ouvert directement.

Retourner sur :

```text
http://localhost:5080
```

Puis recommencer la connexion depuis l'application.

## Production

En production, ajouter dans la meme application Yahoo une URI de redirection supplementaire avec le domaine public Keycloak :

```text
https://auth.example.com/realms/wfrp4/broker/yahoo/endpoint
```

Adapter `auth.example.com` au domaine reel.
