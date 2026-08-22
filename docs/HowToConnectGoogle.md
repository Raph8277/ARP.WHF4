# HowToConnectGoogle

Guide de configuration de la connexion Google via Keycloak pour WFRP4.

## Principe

L'application Blazor ne parle pas directement a Google. Elle redirige vers Keycloak, puis Keycloak agit comme broker OAuth/OIDC avec Google.

En developpement local :
- application WFRP4 : `http://localhost:5080`
- Keycloak : `http://localhost:8080`
- realm : `wfrp4`
- provider Keycloak : `google`

L'URL de callback Google est donc :

```text
http://localhost:8080/realms/wfrp4/broker/google/endpoint
```

Cette URL ne doit pas etre ouverte directement dans le navigateur. Elle sert uniquement a Google pour revenir vers Keycloak apres authentification.

## 1. Creer un client OAuth Google

Dans Google Cloud Console :

1. Ouvrir le projet Google Cloud de l'application.
2. Aller dans **API et services** > **Identifiants**.
3. Cliquer **Creer des identifiants** > **ID client OAuth**.
4. Si Google le demande, configurer d'abord l'ecran de consentement OAuth.
5. Choisir le type d'application **Application Web**.
6. Nommer le client, par exemple `WFRP4 local`.
7. Dans **URI de redirection autorises**, ajouter exactement :

```text
http://localhost:8080/realms/wfrp4/broker/google/endpoint
```

8. Enregistrer.

Google fournit ensuite :
- un **ID client**
- un **code secret du client**

## 2. Configurer les variables locales

Creer le fichier local `app/.env` a partir de `app/.env.example`.

```env
WFRP4_GOOGLE_CLIENT_ID=xxxxx.apps.googleusercontent.com
WFRP4_GOOGLE_CLIENT_SECRET=xxxxx

WFRP4_YAHOO_CLIENT_ID=
WFRP4_YAHOO_CLIENT_SECRET=

WFRP4_META_CLIENT_ID=
WFRP4_META_CLIENT_SECRET=
```

Le fichier `app/.env` contient des secrets et doit rester ignore par Git.

## 3. Appliquer la configuration Keycloak

Depuis `app/` :

```powershell
docker compose up -d keycloak
docker cp keycloak/social-idps/. wfrp4-keycloak:/tmp/wfrp4-social-idps
docker exec wfrp4-keycloak sh /tmp/wfrp4-social-idps/apply-social-idps.sh
```

Le script active Google seulement si `WFRP4_GOOGLE_CLIENT_ID` et `WFRP4_GOOGLE_CLIENT_SECRET` sont renseignes. Yahoo et Meta restent desactives tant que leurs variables sont vides.

Pour verifier l'etat du provider Google :

```powershell
docker exec wfrp4-keycloak /opt/keycloak/bin/kcadm.sh config credentials --server http://localhost:8080 --realm master --user admin --password admin
docker exec wfrp4-keycloak /opt/keycloak/bin/kcadm.sh get identity-provider/instances/google -r wfrp4 --fields alias,enabled
```

Resultat attendu :

```json
{
  "alias": "google",
  "enabled": true
}
```

## 4. Tester la connexion

Ouvrir l'application :

```text
http://localhost:5080
```

Puis cliquer sur la connexion et choisir **Google** dans l'ecran Keycloak.

Ne pas ouvrir directement :

```text
http://localhost:8080/realms/wfrp4/broker/google/endpoint
```

Cette URL est uniquement le callback technique.

## Depannage

### Erreur Google `redirect_uri_mismatch`

Google n'a pas l'URI exacte envoyee par Keycloak.

Verifier dans Google Cloud Console que l'URI suivante est dans **URI de redirection autorises** :

```text
http://localhost:8080/realms/wfrp4/broker/google/endpoint
```

Points a respecter :
- `http`, pas `https`, en local
- `localhost`, pas `127.0.0.1`
- port `8080`
- pas de `/` final
- champ **URI de redirection autorises**, pas uniquement **Origines JavaScript autorisees**

### Erreur Keycloak `Missing state parameter`

Le callback Keycloak a ete ouvert directement.

Retourner sur :

```text
http://localhost:5080
```

Puis recommencer la connexion depuis l'application.

### Erreur Keycloak `Could not create authentication request`

Le provider Google est probablement actif sans `clientId` ou sans `clientSecret`.

Verifier que `app/.env` contient les deux variables Google, puis relancer l'import Keycloak.

## Production

En production, ajouter dans le meme client OAuth Google une URI de redirection supplementaire avec le domaine public Keycloak :

```text
https://auth.example.com/realms/wfrp4/broker/google/endpoint
```

Adapter `auth.example.com` au domaine reel.
