document.addEventListener("DOMContentLoaded", () => {

    const deleteModal = document.getElementById("deleteModal");

    if (!deleteModal)
        return;

    deleteModal.addEventListener("show.bs.modal", function (event) {

        const button = event.relatedTarget;

        const action = button.getAttribute("data-delete-action");
        const message = button.getAttribute("data-delete-message");

        const form = document.getElementById("deleteModalForm");
        const text = document.getElementById("deleteModalMessage");

        form.action = action;

        text.innerText = message;
    });

});