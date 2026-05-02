<script lang="ts">
    interface MedRecord {
        id: number;
        description: string;
        medDate: string;
        am: boolean;
        pm: boolean;
    }
    
    let baseUrl = 'http://medappapi.dandland.com/meds';
    //let baseUrl = 'http://localhost:5063/meds';
    
    let dt = $state(new Date());
    let record: MedRecord | undefined = $state(undefined);
    let webDate:string =  $state("");
    let dtString = $derived(
        `${dt.getFullYear()}-${dt.getMonth() + 1}-${dt.getDate()}`
    );

    const request = $derived.by(async () => 
    {           
        let medRecord: MedRecord;           
        const res = await fetch(`${baseUrl}`, 
        {
            method: 'GET',
            // rmode: 'cors', // This is the default; it tells the browser to check for CORS headers
            // credentials: 'include', // Use this if you need to send cookies or Windows Auth
            headers: 
            {
              'Content-Type': 'application/json',
              'X-DandlandOnly':'dandlandonly'
            }     
        });
                
        if (!res.ok)
        {
            throw new Error(`HTTP ${res.status}`);            
        } 
        record =await res.json() as MedRecord;        
        webDate= `${record.medDate?.split('T')[0]}`;      
        return record;
    });
    
    const valueChanged = async () =>
    {
        try 
        {    
            let medData: MedRecord  = 
            {
                description: record.description,
                id: record.id,
                medDate: record.medDate,
                am:record.am,
                pm:record.pm               
            }          
              
            const response = await fetch(`${baseUrl}`, 
            {
                method: 'PUT',
                //mode: 'cors', // This is the default; it tells the browser to check for CORS headers
                // credentials: 'include', // Use this if you need to send cookies or Windows Auth            
                headers: 
                {
                  'Content-Type': 'application/json',
                  'X-DandlandOnly':'dandlandonly'
                },
                body: JSON.stringify(medData)
            });
                    
            if (response.ok) 
            {                
                record = medData;
            }
            else 
            {
                console.error('Update failed:', response.statusText);                
            } 
        }
        catch (error) 
        {
            console.error('Network error:', error);
        }
      }
  
        const dateChange = async () => 
        {
            // 1. Fix the URL (added the slash)
            const res = await fetch(`${baseUrl}/${webDate}`, 
            {
                method: 'GET', // Should this be GET? Usually fetching a record by date is GET
                headers: 
                {
                    'Content-Type': 'application/json',
                    'X-DandlandOnly': 'dandlandonly'
                }
            });       
            
            if (res.ok) 
            {
                const tmpRecord = await res.json();
                // 2. Reassign the whole object to ensure Svelte sees the change
                record = tmpRecord; 
            }
        };
//        const dateChange = async() =>
//        {          
//            console.table(webDate);             
//            const res = await fetch(`${baseUrl}/${webDate}`, 
//            {
//                method: 'PUT',
//                //mode: 'cors', // This is the default; it tells the browser to check for CORS headers
//                // credentials: 'include', // Use this if you need to send cookies or Windows Auth                
//                headers: 
//                {
//                  'Content-Type': 'application/json',
//                  'X-DandlandOnly':'dandlandonly'
//                },
//                body: JSON.stringify(medData)
//            });            
//                        
//            if (!res.ok)
//            {
//                throw new Error(`HTTP ${res.status}`);                
//            } 
//            let tmpRecord:MedRecord =await res.json() as MedRecord;
//            record.id  = tmpRecord.id;
//            record.description  = tmpRecord.description;
//            record.medDate  = tmpRecord?.medDate;
//            record.am  = tmpRecord.am;
//            record.pm  = tmpRecord.pm;           
//            // webDate= `${record.medDate?.split('T')[0]}`;            
//            return record;
//        }
    
</script>

{#await request}
    <p>Loading...</p>
{:then _}
    <div>
        <label for='description'>Description</label>
        <input id='description' type="text" bind:value={record.description} onchange={valueChanged} />
    </div>

    <div>
        <label for='dateValue'>Log Date</label>
        <input id="dateValue" type="date" bind:value={webDate} onchange={dateChange}/>
    </div>

    <div>
        <label for='am'>AM:</label>
        <input id='am'type="checkbox" bind:checked={record.am} onchange={valueChanged} />
    </div>

    <div>
        <label for='pm'>PM:</label>
        <input id='pm' type="checkbox" bind:checked={record.pm} onchange={valueChanged} />
    </div>

{:catch err}
    <p style="color:red">Error: {err.message}</p>
{/await}

 