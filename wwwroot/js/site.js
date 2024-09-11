// Resend verification key
$(document).ready(function () {
    $('#ResendKeyWithEmail').on('submit', function (e) {
        e.preventDefault();

        Swal.fire({
            title: 'Onaylıyor musunuz?',
            text: 'Onay verdiğinizde kullanıcının mailine kod gidecektir.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Evet, gönder!',
            cancelButtonText: 'İptal'
        }).then((result) => {
            if (result.isConfirmed) {
                var formData = $(e.target).serialize();
                $.ajax({
                    type: 'POST',
                    url: $(e.target).attr('action'),
                    data: formData,
                    success: function (response) {
                        Swal.fire({
                            title: 'Başarılı!',
                            text: 'Kod başarıyla gönderildi.',
                            icon: 'success',
                            confirmButtonText: 'Tamam'
                        });
                    },
                    error: function (xhr, status, error) {
                        Swal.fire({
                            title: 'Hata!',
                            text: 'Kod gönderilirken bir hata oluştu.',
                            icon: 'error',
                            confirmButtonText: 'Tamam'
                        });
                    }
                });
            }
        });
    });
});

// Delete company
$(document).ready(function () {
    $('#DeleteCompany').on('submit', function (e) {
        e.preventDefault();

        Swal.fire({
            title: 'Onaylıyor musunuz?',
            text: 'Onay verdiğinizde silinecektir.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Evet, sil!',
            cancelButtonText: 'İptal'
        }).then((result) => {
            if (result.isConfirmed) {
                var formData = $(e.target).serialize();
                $.ajax({
                    type: 'POST',
                    url: $(e.target).attr('action'),
                    data: formData,
                    success: function (response) {
                        Swal.fire({
                            title: 'Başarılı!',
                            text: 'Hesap başarıyla silindi.',
                            icon: 'success',
                            confirmButtonText: 'Tamam'
                        });
                    },
                    error: function (xhr, status, error) {
                        Swal.fire({
                            title: 'Hata!',
                            text: 'Hesap silinirken bir hata oluştu.',
                            icon: 'error',
                            confirmButtonText: 'Tamam'
                        });
                    }
                });
            }
        });
    });
});