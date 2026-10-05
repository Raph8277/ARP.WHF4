# HowToConnectMeta

Guide de configuration de la connexion Meta (Facebook Login) via Keycloak pour WFRP4.

## Principe

L'application Blazor ne parle pas directement a Meta. Elle redirige vers Keycloak, puis Keycloak agit comme broker OAuth avec Meta (provider Keycloak `facebook`, alias `meta`).

En developpement local :
- application WFRP4 : `http://localhost:5080`
- Keycloak : `http://localhost:8080`
- realm : `wfrp4`
- provider Keycloak : `meta`

L'URL de callback Meta est donc :

```text
http://localhost:8080/realms/wfrp4/broker/meta/endpoint
```

Cette URL ne doit pas etre ouverte directement dans le navigateur. Elle sert uniquement a Meta pour revenir vers Keycloak apres authentification.

## 1. Creer une application Meta

Dans Meta for Developers (`https://developers.facebook.com/apps/`) :

1. Cliquer **Creer une app**.
2. Choisir le type d'usage **Consommateur** (ou **Autre** selon les options proposees).
3. Nommer l'application, par exemple `WFRP4 local`.
4. Dans le tableau de bord de l'app, ajouter le produit **Facebook Login**.
5. Dans **Facebook Login** > **Parametres**, champ **URI de redirection OAuth valides**, ajouter exactement :

```text
http://localhost:8080/realms/wfrp4/broker/meta/endpoint
```

6. Enregistrer les modifications.

Dans **Parametres de l'app** > **Informations de base**, Meta fournit :
- un **ID de l'application**
- une **Cle secrete de l'application**

Ces deux valeurs restent consultables sur `https://developers.facebook.com/apps/`, en ouvrant l'application puis **Parametres** > **Informations de base** ; la cle secrete peut y etre regeneree si necessaire.

Tant que l'application reste en mode **Developpement**, seuls les comptes definis comme testeurs/administrateurs/developpeurs de l'app (dans **Roles de l'application**) peuvent se connecter. Ajouter les comptes de test necessaires, ou passer l'app en mode **Live** apres validation Meta pour ouvrir la connexion a tout utilisateur.

## 2. Configurer les variables locales

Creer le fichier local `app/.env` a partir de `app/.env.example`.

```env
WFRP4_GOOGLE_CLIENT_ID=
WFRP4_GOOGLE_CLIENT_SECRET=

WFRP4_YAHOO_CLIENT_ID=
WFRP4_YAHOO_CLIENT_SECRET=

WFRP4_META_CLIENT_ID=xxxxx
WFRP4_META_CLIENT_SECRET=xxxxx

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

Le script active Meta seulement si `WFRP4_META_CLIENT_ID` et `WFRP4_META_CLIENT_SECRET` sont renseignes. Google, Yahoo et Microsoft suivent la meme regle independamment.

Pour verifier l'etat du provider Meta :

```powershell
docker exec wfrp4-keycloak /opt/keycloak/bin/kcadm.sh config credentials --server http://localhost:8080 --realm master --user admin --password admin
docker exec wfrp4-keycloak /opt/keycloak/bin/kcadm.sh get identity-provider/instances/meta -r wfrp4 --fields alias,enabled
```

Resultat attendu :

```json
{
  "alias": "meta",
  "enabled": true
}
```

## 4. Tester la connexion

Ouvrir l'application :

```text
http://localhost:5080
```

Puis cliquer sur la connexion et choisir **Meta** dans l'ecran Keycloak.

Ne pas ouvrir directement :

```text
http://localhost:8080/realms/wfrp4/broker/meta/endpoint
```

Cette URL est uniquement le callback technique.

## Depannage

### Erreur Meta `URL bloquee` ou `Impossible de charger l'URL`

Meta n'a pas l'URI exacte envoyee par Keycloak.

Verifier dans **Facebook Login** > **Parametres** que l'URI suivante est dans **URI de redirection OAuth valides** :

```text
http://localhost:8080/realms/wfrp4/broker/meta/endpoint
```

Points a respecter :
- `http`, pas `https`, en local
- `localhost`, pas `127.0.0.1`
- port `8080`
- pas de `/` final

### Erreur Meta `L'app n'est pas disponible actuellement`

Le compte utilise pour tester la connexion n'a pas de role sur l'application Meta et l'app est encore en mode **Developpement**.

Ajouter le compte dans **Roles de l'application** (testeur, developpeur ou administrateur), ou passer l'app en mode **Live**.

### Erreur Keycloak `Missing state parameter`

Le callback Keycloak a ete ouvert directement.

Retourner sur :

```text
http://localhost:5080
```

Puis recommencer la connexion depuis l'application.

## Production

En production, ajouter dans la meme application Meta une URI de redirection supplementaire avec le domaine public Keycloak :

```text
https://auth.example.com/realms/wfrp4/broker/meta/endpoint
```

Adapter `auth.example.com` au domaine reel. Verifier egalement que l'app Meta est en mode **Live** pour accepter tous les utilisateurs.
