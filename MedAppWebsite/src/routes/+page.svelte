<script lang="ts">
    interface MedRecord {
        id: number;
        description: string;
        medDate: string;
        am: boolean;
        pm: boolean;
    }

    const baseUrl = 'http://medappapi.dandland.com/meds';
    
    // 1. State: The source of truth is the date string
    let webDate = $state(new Date().toISOString().split('T')[0]);
    
    // 2. Reactive Data Fetching: 
    // This automatically re-runs whenever 'webDate' changes.
    let recordPromise = $derived.by(() => fetchRecord(webDate));

    async function fetchRecord(date: string) 
    {
        const res = await fetch(`${baseUrl}/${date}`, 
        {
            headers: 
            {
                'X-DandlandOnly': 'dandlandonly' 
            }
        });
        if (!res.ok){
            throw new Error(`Could not fetch record for ${date}`);            
        } 
        return await res.json() as MedRecord;
    }

    // 3. Update Function:
    // We pass the current record to the function to keep it clean.
    async function updateRecord(record: MedRecord) 
    {
        try 
        {
            const response = await fetch(baseUrl, 
            {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'X-DandlandOnly': 'dandlandonly'
                },
                body: JSON.stringify(record)
            });
            
            if (!response.ok) 
            {
                console.error('Update failed');
            }
        } 
        catch (error) 
        {
            console.error('Network error:', error);
        }
    }
    
</script>

{#await recordPromise}
    <p>Loading record for {webDate}...</p>
{:then record}
    <div class="form">
        <div>
            <label for='dateValue'>Log Date</label>
            <!-- Changing this triggers 'recordPromise' to re-run automatically -->
            <input id="dateValue" type="date" bind:value={webDate} />
        </div>
        
        <div>
            <label for='description'>Description</label>
            <input id='description' type="text" bind:value={record.description} onchange={() => updateRecord(record)} 
            />
        </div>

        <div>
            <label for='am'>AM:</label>
            <input id='am' type="checkbox" bind:checked={record.am} onchange={() => updateRecord(record)} 
            />
        </div>

        <div>
            <label for='pm'>PM:</label>
            <input id='pm' type="checkbox" bind:checked={record.pm} onchange={() => updateRecord(record)} 
            />
        </div>
    </div>
{:catch err}
    <div>
        <p style="color:red">{err.message}</p>
        <label>Try another date: </label>
        <input type="date" bind:value={webDate} />
    </div>
{/await}

<style>
    .form 
    { 
        display: flex; 
        flex-direction: column; 
        gap: 1rem; 
        margin-top: 1rem; 
    }
    
    hr 
    { 
        width: 100%; 
        border: 0; 
        border-top: 1px solid #ccc; 
    }
    
</style>