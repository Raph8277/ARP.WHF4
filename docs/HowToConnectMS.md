# HowToConnectMS

Guide de configuration de la connexion Microsoft via Keycloak pour WFRP4.

## Principe

L'application Blazor ne parle pas directement a Microsoft. Elle redirige vers Keycloak, puis Keycloak agit comme broker OAuth/OIDC avec Microsoft Entra ID (provider Keycloak integre `microsoft`, alias `microsoft`).

En developpement local :
- application WFRP4 : `http://localhost:5080`
- Keycloak : `http://localhost:8080`
- realm : `wfrp4`
- provider Keycloak : `microsoft`

L'URL de callback Microsoft est donc :

```text
http://localhost:8080/realms/wfrp4/broker/microsoft/endpoint
```

Cette URL ne doit pas etre ouverte directement dans le navigateur. Elle sert uniquement a Microsoft pour revenir vers Keycloak apres authentification.

## 1. Creer une inscription d'application Microsoft Entra ID

Dans le portail Azure, page **Inscriptions d'applications** (`https://portal.azure.com/#view/Microsoft_AAD_RegisteredApps/ApplicationsListBlade`) :

1. Ouvrir **Microsoft Entra ID** > **Inscriptions d'applications**.
2. Cliquer **Nouvelle inscription**.
3. Nommer l'application, par exemple `WFRP4 local`.
4. Dans **Types de comptes pris en charge**, choisir **Comptes dans un annuaire d'organisation et comptes Microsoft personnels** (sauf si le jeu doit rester limite a une seule organisation).
5. Dans **URI de redirection**, choisir la plateforme **Web** et renseigner exactement :

```text
http://localhost:8080/realms/wfrp4/broker/microsoft/endpoint
```

6. Cliquer **Inscrire**.

Sur la page **Vue d'ensemble** de l'application, Microsoft fournit l'**ID d'application (client)**. Cette page reste accessible a tout moment depuis `https://portal.azure.com/#view/Microsoft_AAD_RegisteredApps/ApplicationsListBlade`, en reselectionnant l'application.

Pour le secret, aller dans **Certificats et secrets** > **Nouveau secret client**, creer un secret et copier immediatement sa **valeur** (elle n'est plus visible ensuite). Si la valeur est perdue, un nouveau secret doit etre cree depuis cette meme page ; l'ancien peut etre supprime.

## 2. Configurer les variables locales

Creer le fichier local `app/.env` a partir de `app/.env.example`.

```env
WFRP4_GOOGLE_CLIENT_ID=
WFRP4_GOOGLE_CLIENT_SECRET=

WFRP4_YAHOO_CLIENT_ID=
WFRP4_YAHOO_CLIENT_SECRET=

WFRP4_META_CLIENT_ID=
WFRP4_META_CLIENT_SECRET=

WFRP4_MS_CLIENT_ID=xxxxx
WFRP4_MS_CLIENT_SECRET=xxxxx
```

Le fichier `app/.env` contient des secrets et doit rester ignore par Git.

## 3. Appliquer la configuration Keycloak

Depuis `app/` :

```powershell
docker compose up -d keycloak
docker cp keycloak/social-idps/. wfrp4-keycloak:/tmp/wfrp4-social-idps
docker exec wfrp4-keycloak sh /tmp/wfrp4-social-idps/apply-social-idps.sh
```

Le script active Microsoft seulement si `WFRP4_MS_CLIENT_ID` et `WFRP4_MS_CLIENT_SECRET` sont renseignes. Google, Yahoo et Meta suivent la meme regle independamment.

Pour verifier l'etat du provider Microsoft :

```powershell
docker exec wfrp4-keycloak /opt/keycloak/bin/kcadm.sh config credentials --server http://localhost:8080 --realm master --user admin --password admin
docker exec wfrp4-keycloak /opt/keycloak/bin/kcadm.sh get identity-provider/instances/microsoft -r wfrp4 --fields alias,enabled
```

Resultat attendu :

```json
{
  "alias": "microsoft",
  "enabled": true
}
```

## 4. Tester la connexion

Ouvrir l'application :

```text
http://localhost:5080
```

Puis cliquer sur la connexion et choisir **Microsoft** dans l'ecran Keycloak.

Ne pas ouvrir directement :

```text
http://localhost:8080/realms/wfrp4/broker/microsoft/endpoint
```

Cette URL est uniquement le callback technique.

## Depannage

### Erreur Microsoft `AADSTS50011` (redirect URI mismatch)

Microsoft n'a pas l'URI exacte envoyee par Keycloak.

Verifier dans l'inscription d'application, sous **Authentification**, que l'URI suivante est enregistree sur la plateforme **Web** :

```text
http://localhost:8080/realms/wfrp4/broker/microsoft/endpoint
```

Points a respecter :
- `http`, pas `https`, en local
- `localhost`, pas `127.0.0.1`
- port `8080`
- pas de `/` final
- plateforme **Web**, pas **SPA** (une redirection SPA n'accepte pas ce flux cote serveur)

### Erreur Microsoft `AADSTS7000215` (secret client invalide)

Le secret configure a expire ou a ete mal copie.

Creer un nouveau secret dans **Certificats et secrets**, puis mettre a jour `WFRP4_MS_CLIENT_SECRET` dans `app/.env` et relancer l'import Keycloak.

### Erreur Keycloak `Missing state parameter`

Le callback Keycloak a ete ouvert directement.

Retourner sur :

```text
http://localhost:5080
```

Puis recommencer la connexion depuis l'application.

## Production

En production, ajouter dans la meme inscription d'application Microsoft une URI de redirection supplementaire avec le domaine public Keycloak :

```text
https://auth.example.com/realms/wfrp4/broker/microsoft/endpoint
```

Adapter `auth.example.com` au domaine reel.
