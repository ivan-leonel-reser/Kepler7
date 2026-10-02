# Cómo sumarte al repo

Trabajamos todos desde la PC del grupo. Para que tus commits, tus push y tus PR salgan con **tu** cuenta de GitHub hay que hacer esto una sola vez. No hace falta pasar contraseñas ni tokens.

## 1. Cuenta de GitHub

Si no tenés, creala en https://github.com/signup (es gratis).

## 2. Pasale a Ivan tu usuario

Solo el usuario de GitHub (el que aparece en tu perfil, ej. `juanperez`). No hace falta el mail: los commits usan la dirección `@users.noreply.github.com` de tu cuenta, que GitHub siempre vincula a tu perfil aunque tengas el mail privado.

## 3. Aceptá la invitación al repo

Te llega un mail de GitHub. Si no lo encontrás, entrá con tu cuenta a https://github.com/ivan-leonel-reser/Kepler7/invitations y tocá *Accept invitation*. Sin esto tu cuenta no puede pushear.

## 4. Autorizá tu cuenta en la PC del grupo

Se coordina con Ivan, lleva un minuto y se puede hacer desde el celular:

1. Ivan corre un comando en la PC y te manda un código de 8 caracteres (tipo `ABCD-1234`).
2. Entrás a https://github.com/login/device con **tu** cuenta abierta.
3. Pegás el código y tocás *Authorize GitHub CLI*.

El código vence a los 15 minutos.

## 5. Cuando te toca trabajar

Avisás que te sentás. Los commits salen con tu nombre y tu mail, y el push y el PR con tu cuenta. Antes de pushear se revisa que el autor sea el correcto:

```bash
git log -3 --format="%an <%ae> %s"
```

Tu parte se trabaja en varios commits (uno por paso que compila) y tiene que quedar funcionando en el juego antes del PR. Ver [REGLAS.md](REGLAS.md).

## Para dar de baja el acceso

https://github.com/settings/applications → *GitHub CLI* → *Revoke*.

---

## Para Ivan: dar de alta una cuenta en la PC

Una vez por integrante:

```bash
gh auth login --hostname github.com --git-protocol https --web
```

1. Copiá el código que muestra y mandáselo al compañero.
2. Al apretar Enter se abre una pestaña del navegador con tu sesión: **cerrala sin autorizar nada**. El que autoriza es el compañero, desde su celular.
3. Cuando él autoriza, la terminal muestra `Logged in as <su-usuario>`.

Después verificá y dejá activa tu cuenta:

```bash
gh auth status
```

```bash
gh auth switch
```
