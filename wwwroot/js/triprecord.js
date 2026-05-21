function showAddTripModal() {
    const vid = GetVehicleId().vehicleId;
    const html = `<div class="modal-header"><h5>Fahrt</h5><button class="btn-close" data-bs-dismiss="modal"></button></div>
<div class="modal-body"><input id="tripDesc" class="form-control mb-2" placeholder="Beschreibung"/>
<textarea id="tripText" class="form-control mb-2" rows="6" placeholder="Details"></textarea>
<label class="form-label">GPX/Dateien</label><input type="file" multiple class="form-control" onchange="uploadVehicleFilesAsync(this)"><div id="filesToUpload"></div></div>
<div class="modal-footer"><button class="btn btn-primary" onclick="saveTrip(${vid},0)">Speichern</button></div>`;
    $("#tripModalContent").html(html);
    $('#tripModal').modal('show');
}
function showEditTripModal(id){ $.get(`/Vehicle/GetNoteForEditById?noteId=${id}`, d=> {$("#noteModalContent").html(d); $('#noteModal').modal('show');}); }
function saveTrip(vehicleId,id){
    const payload={id:id,vehicleId:vehicleId,description:$('#tripDesc').val(),noteText:$('#tripText').val(),files:uploadedFiles,tags:['trip']};
    $.post('/Vehicle/SaveTripToVehicleId',{note:payload},r=>{ if(r.success){$('#tripModal').modal('hide');getVehicleTrips(vehicleId);} else errorToast(r.message);});
}
function importTripsCsv(input){
    const file=input.files[0]; if(!file) return;
    const reader=new FileReader();
    reader.onload=()=>{$.post('/Vehicle/ImportTripsCsv',{vehicleId:GetVehicleId().vehicleId,csvContent:reader.result},r=>{ if(r.success){successToast(r.message);getVehicleTrips(GetVehicleId().vehicleId);} else errorToast(r.message);});};
    reader.readAsText(file);
}
