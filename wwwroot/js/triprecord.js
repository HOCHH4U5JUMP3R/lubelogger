function showAddTripModal() {
    const vehicle = GetVehicleId();
    $.get(`/Vehicle/GetTripModal?vehicleId=${vehicle.vehicleId}`, function (data) {
        if (data) {
            $("#tripModalContent").html(data);
            $('#tripModal').modal('show');
        }
    });
}
function showEditTripModal(id) {
    const vehicle = GetVehicleId();
    $.get(`/Vehicle/GetTripModal?vehicleId=${vehicle.vehicleId}&noteId=${id}`, function (data) {
        if (data) {
            $("#tripModalContent").html(data);
            $('#tripModal').modal('show');
        }
    });
}
function saveTrip(vehicleId, id) {
    const payload = { id: id, vehicleId: vehicleId, description: $('#tripDesc').val(), noteText: $('#tripText').val(), files: uploadedFiles, tags: ['trip'] };
    $.post('/Vehicle/SaveTripToVehicleId', { note: payload }, function (r) {
        if (r.success) {
            $('#tripModal').modal('hide');
            getVehicleTrips(vehicleId);
        } else {
            errorToast(r.message);
        }
    });
}
function importTripsCsv(input) {
    const file = input.files[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = function () {
        $.post('/Vehicle/ImportTripsCsv', { vehicleId: GetVehicleId().vehicleId, csvContent: reader.result }, function (r) {
            if (r.success) {
                successToast(r.message);
                getVehicleTrips(GetVehicleId().vehicleId);
            } else {
                errorToast(r.message);
            }
        });
    };
    reader.readAsText(file);
}
