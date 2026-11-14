document.addEventListener("DOMContentLoaded", function() {
    const button = document.getElementById("myButton");
    button.addEventListener("click", function() {
        alert("¡Hola! Has hecho clic en el botón.");
    });

    const formPost = document.getElementById("formPost");
    const modal = document.getElementById("modalRecibido");
    const titulo = document.getElementById("modalTitulo");
    const mensaje = document.getElementById("modalMensaje");

    document.getElementById("modalCerrar").addEventListener("click", function() {
        modal.close();
    });

    formPost.addEventListener("submit", async function(evento) {
        evento.preventDefault();
        const datos = new URLSearchParams(new FormData(formPost));
        try {
            const respuesta = await fetch(formPost.action, { method: "POST", body: datos });
            if (respuesta.ok) {
                titulo.textContent = "Datos recibidos";
                mensaje.textContent = "El servidor recibió: " + datos.toString();
                formPost.reset();
            } else {
                titulo.textContent = "No se pudo enviar";
                mensaje.textContent = "El servidor respondió con el código " + respuesta.status + ".";
            }
        } catch (error) {
            titulo.textContent = "No se pudo enviar";
            mensaje.textContent = "No hubo respuesta del servidor.";
        }
        modal.showModal();
    });
});
