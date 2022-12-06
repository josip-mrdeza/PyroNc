pub struct CutResult
{
    pub total_time: i64,
    pub total_vertices_cut: i64,
    pub threw: bool
}

impl CutResult{
    extern fn new(total_t: i64, total_verts_cut: i64, threw: bool) -> CutResult{
        return CutResult{
            total_time: total_t,
            total_vertices_cut: total_verts_cut,
            threw
        };
    }
}