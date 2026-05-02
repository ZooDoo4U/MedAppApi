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

    const request = $derived.by(async () => {
        // const res = await fetch(`${baseUrl}/${dtString}`);
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
            // body: JSON.stringify(medData)
        });
        console.log(`date ${dtString}`);
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        record =await res.json() as MedRecord;
        console.log(`datavalue == [${record.medDate}]`);
        webDate= `${record.medDate.split('T')[0]}`;
        console.log(`wdbDate == [${webDate}]`);        
        console.table(record);
        return record;
    });
    
    const valueChanged = async () =>
    {
        console.log(`date changed  id == ${record?.id}`);
        debugger;
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
            
            console.table(medData);
            
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
                console.log('Update successful!');
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
      
        const dateChange = async() =>
        {            
            console.log(`webdate changed... [${webDate}]`);
            // const res = await fetch(`${baseUrl}/${webDate}`);
                     
            const res = await fetch(`${baseUrl}`, 
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
            
            
            console.log(`date ${dtString}`);
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            console.table(record);
            let tmpRecord:MedRecord =await res.json() as MedRecord;
            console.table(tmpRecord);                        
            record.id  = tmpRecord.id;
            record.description  = tmpRecord.description;
            record.medDate  = tmpRecord.medDate;
            record.am  = tmpRecord.am;
            record.pm  = tmpRecord.pm;           
            console.table(record);                       
            console.log(`datavalue == [${record.medDate}]`);
            webDate= `${record.medDate.split('T')[0]}`;
            console.log(`webDate == [${webDate}]`);        
            console.table(record);
            debugger;
            return record;
        }
    
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

 